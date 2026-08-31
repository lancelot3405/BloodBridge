"""FastAPI router for seven-day blood-demand forecasts."""

from datetime import timedelta
from pathlib import Path

import joblib
import pandas as pd
from fastapi import APIRouter, HTTPException


router = APIRouter()
FOLDER = Path(__file__).parent
DATASET_PATH = FOLDER / "inventory_dataset.csv"
MODEL_PATH = FOLDER / "demand_model.pkl"


# Load the saved model and source data when a forecast is requested.
def load_forecast_assets():
    """Load the trained model package and historical demand data."""

    if not DATASET_PATH.exists() or not MODEL_PATH.exists():
        raise HTTPException(
            status_code=503,
            detail="Inventory model is not ready. Run generate_timeseries.py and train_forecaster.py first.",
        )

    model_package = joblib.load(MODEL_PATH)
    dataset = pd.read_csv(DATASET_PATH)
    dataset["Date"] = pd.to_datetime(dataset["Date"])
    return model_package, dataset


# Predict one future day using the previous prediction when a future lag is needed.
def predict_next_day(model, blood_group_code: int, date, previous_demands: list[float]) -> float:
    """Return the next demand value for one blood group."""

    demand_t_minus_1 = previous_demands[-1]
    demand_t_minus_7 = previous_demands[-7] if len(previous_demands) >= 7 else previous_demands[0]
    row = pd.DataFrame(
        [
            {
                "Blood_Group_Code": blood_group_code,
                "Day_Of_Week": date.dayofweek,
                "Is_Weekend": int(date.dayofweek >= 5),
                "Demand_T_minus_1": demand_t_minus_1,
                "Demand_T_minus_7": demand_t_minus_7,
            }
        ]
    )
    return max(0.0, round(float(model.predict(row)[0]), 2))


# Return a seven-day list so the C# dashboard can draw a simple line chart.
@router.get("/forecast-demand/{blood_group}")
def forecast_demand(blood_group: str) -> list[dict]:
    """Forecast the next seven days for the requested blood group."""

    model_package, dataset = load_forecast_assets()
    normalized_group = blood_group.strip().upper()
    blood_groups = model_package["blood_groups"]
    if normalized_group not in blood_groups:
        raise HTTPException(
            status_code=400,
            detail=f"Unknown blood group. Use one of: {', '.join(blood_groups)}",
        )

    model = model_package["model"]
    blood_group_code = blood_groups.index(normalized_group)
    group_history = dataset[dataset["Blood_Group"] == normalized_group].sort_values("Date")
    if group_history.empty:
        raise HTTPException(status_code=404, detail="No historical demand exists for this blood group.")

    previous_demands = group_history["Demand"].astype(float).tolist()
    next_date = group_history["Date"].max() + timedelta(days=1)
    forecast = []

    for _ in range(7):
        predicted_demand = predict_next_day(
            model,
            blood_group_code,
            next_date,
            previous_demands,
        )
        forecast.append(
            {
                "date": next_date.strftime("%Y-%m-%d"),
                "blood_group": normalized_group,
                "predicted_demand": predicted_demand,
            }
        )
        previous_demands.append(predicted_demand)
        next_date += timedelta(days=1)

    return forecast
