"""Train an XGBoost model that forecasts daily blood demand."""

from pathlib import Path

import joblib
import pandas as pd
from sklearn.metrics import mean_absolute_error, mean_squared_error
from xgboost import XGBRegressor


FEATURE_NAMES = [
    "Blood_Group_Code",
    "Day_Of_Week",
    "Is_Weekend",
    "Demand_T_minus_1",
    "Demand_T_minus_7",
]
BLOOD_GROUPS = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"]


# Add calendar and lag values because recent demand and weekends help explain future demand.
def add_features(dataset: pd.DataFrame) -> pd.DataFrame:
    """Return a dataset containing the features used by XGBoost."""

    result = dataset.copy()
    result["Date"] = pd.to_datetime(result["Date"])
    result["Blood_Group_Code"] = result["Blood_Group"].map(
        {blood_group: index for index, blood_group in enumerate(BLOOD_GROUPS)}
    )
    result["Day_Of_Week"] = result["Date"].dt.dayofweek
    result["Is_Weekend"] = (result["Day_Of_Week"] >= 5).astype(int)

    # Grouping before shifting prevents one blood group from using another group's history.
    result["Demand_T_minus_1"] = result.groupby("Blood_Group")["Demand"].shift(1)
    result["Demand_T_minus_7"] = result.groupby("Blood_Group")["Demand"].shift(7)
    return result.dropna(subset=FEATURE_NAMES + ["Demand"])


# Train, measure, and save the forecaster so FastAPI can use it later.
def train_forecaster() -> Path:
    """Train XGBRegressor, print MAE/RMSE, and save demand_model.pkl."""

    folder = Path(__file__).parent
    dataset_path = folder / "inventory_dataset.csv"
    model_path = folder / "demand_model.pkl"

    if not dataset_path.exists():
        raise FileNotFoundError(
            "inventory_dataset.csv was not found. Run generate_timeseries.py first."
        )

    # Sort by date so the test set represents later dates for every blood group.
    dataset = add_features(pd.read_csv(dataset_path)).sort_values(
        ["Date", "Blood_Group"]
    ).reset_index(drop=True)
    features = dataset[FEATURE_NAMES]
    target = dataset["Demand"]

    # A chronological split is easy to explain and resembles a real future-forecast test.
    split_index = int(len(dataset) * 0.8)
    train_features = features.iloc[:split_index]
    test_features = features.iloc[split_index:]
    train_target = target.iloc[:split_index]
    test_target = target.iloc[split_index:]

    model = XGBRegressor(
        n_estimators=200,
        max_depth=4,
        learning_rate=0.05,
        objective="reg:squarederror",
        random_state=42,
    )
    model.fit(train_features, train_target)

    predictions = model.predict(test_features)
    mae = mean_absolute_error(test_target, predictions)
    rmse = mean_squared_error(test_target, predictions) ** 0.5
    print(f"MAE: {mae:.3f}")
    print(f"RMSE: {rmse:.3f}")

    # Save metadata with the model so the router uses the same feature order.
    joblib.dump(
        {
            "model": model,
            "feature_names": FEATURE_NAMES,
            "blood_groups": BLOOD_GROUPS,
        },
        model_path,
    )
    print(f"Saved trained model: {model_path}")
    return model_path


# Run training when this file is started directly from the command line.
if __name__ == "__main__":
    train_forecaster()
