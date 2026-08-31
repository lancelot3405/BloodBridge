"""Train and save a simple Random Forest donor-response model."""

from pathlib import Path

import joblib
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, roc_auc_score
from sklearn.model_selection import train_test_split


# Train the model using the same feature names that the FastAPI app will send later.
def train_model() -> Path:
    """Train the classifier, print basic results, and save the model."""

    folder = Path(__file__).parent
    dataset_path = folder / "donor_dataset.csv"
    model_path = folder / "donor_model.pkl"

    if not dataset_path.exists():
        raise FileNotFoundError(
            "donor_dataset.csv was not found. Run generate_dataset.py first."
        )

    dataset = pd.read_csv(dataset_path)
    feature_names = [
        "Distance_KM",
        "Requests_Received",
        "Requests_Accepted",
        "Gamification_XP",
    ]
    features = dataset[feature_names]
    target = dataset["Will_Respond"]

    # Stratification keeps both response classes represented in the train and test sets.
    train_features, test_features, train_target, test_target = train_test_split(
        features,
        target,
        test_size=0.2,
        random_state=42,
        stratify=target,
    )

    # A Random Forest is easy to explain and is sufficient for this academic prototype.
    model = RandomForestClassifier(
        n_estimators=150,
        random_state=42,
        class_weight="balanced",
    )
    model.fit(train_features, train_target)

    predictions = model.predict(test_features)
    probabilities = model.predict_proba(test_features)[:, 1]
    accuracy = accuracy_score(test_target, predictions)
    roc_auc = roc_auc_score(test_target, probabilities)

    print(f"Accuracy: {accuracy:.3f}")
    print(f"ROC-AUC: {roc_auc:.3f}")

    joblib.dump(model, model_path)
    print(f"Saved trained model: {model_path}")
    return model_path


# Run training when the file is started directly from the command line.
if __name__ == "__main__":
    train_model()
