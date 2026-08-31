"""FastAPI service that ranks medically-eligible donors for BloodBridge."""

from pathlib import Path
from typing import List

import joblib
import pandas as pd
from fastapi import FastAPI
from pydantic import BaseModel, Field


MODEL_PATH = Path(__file__).parent / "donor_model.pkl"


# Loading the model once at startup avoids loading the file for every API request.
def load_model():
    """Load the model trained by train_model.py."""

    if not MODEL_PATH.exists():
        raise FileNotFoundError(
            "donor_model.pkl was not found. Run generate_dataset.py and train_model.py first."
        )
    return joblib.load(MODEL_PATH)


# These fields match the small JSON contract used by the C# API adapter.
class DonorInput(BaseModel):
    id: int
    distance: float = Field(ge=0)
    received: int = Field(default=0, ge=0)
    accepted: int = Field(default=0, ge=0)
    xp: float = Field(default=0, ge=0)


# This response includes the model score and a plain-English explanation for the UI or API caller.
class RankedDonor(BaseModel):
    id: int
    distance: float
    received: int
    accepted: int
    xp: float
    response_rate: float
    probability: float
    explanation: str


app = FastAPI(title="BloodBridge Donor Ranking Service")
model = load_model()


# Build a short explanation from donor facts so the model result is easier for a student to present.
def build_explanation(donor: DonorInput, response_rate: float) -> str:
    """Return human-readable reasons that support the ranking result."""

    reasons = []

    if donor.xp >= 700:
        reasons.append("high gamification XP")
    elif donor.xp >= 400:
        reasons.append("good gamification XP")

    if response_rate >= 0.80:
        reasons.append("a 80%+ past response rate")
    elif response_rate >= 0.50:
        reasons.append("a steady past response rate")

    if donor.distance <= 10:
        reasons.append("a short travel distance")
    elif donor.distance <= 25:
        reasons.append("a nearby location")

    if not reasons:
        reasons.append("the combined donor features")

    return "Recommended due to " + " and ".join(reasons) + "."


# Rank every donor with the same four features used during model training.
def rank_donors(donors: List[DonorInput]) -> List[RankedDonor]:
    """Calculate probabilities, add explanations, and sort highest probability first."""

    if not donors:
        return []

    feature_rows = pd.DataFrame(
        [
            {
                "Distance_KM": donor.distance,
                "Requests_Received": donor.received,
                "Requests_Accepted": donor.accepted,
                "Gamification_XP": donor.xp,
            }
            for donor in donors
        ]
    )
    probabilities = model.predict_proba(feature_rows)[:, 1]

    ranked_donors = []
    for donor, probability in zip(donors, probabilities):
        response_rate = donor.accepted / donor.received if donor.received else 0.0
        ranked_donors.append(
            RankedDonor(
                id=donor.id,
                distance=donor.distance,
                received=donor.received,
                accepted=donor.accepted,
                xp=donor.xp,
                response_rate=round(response_rate, 4),
                probability=round(float(probability), 4),
                explanation=build_explanation(donor, response_rate),
            )
        )

    return sorted(ranked_donors, key=lambda donor: donor.probability, reverse=True)


# This endpoint is called by the ASP.NET Core API after it has applied medical eligibility rules.
@app.post("/rank-donors", response_model=List[RankedDonor])
def rank_donors_endpoint(donors: List[DonorInput]) -> List[RankedDonor]:
    """Return eligible donors ordered by predicted response probability."""

    return rank_donors(donors)
