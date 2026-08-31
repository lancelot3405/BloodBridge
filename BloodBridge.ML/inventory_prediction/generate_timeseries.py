"""Generate synthetic daily blood-demand data for the student prototype."""

from pathlib import Path

import numpy as np
import pandas as pd


BLOOD_GROUPS = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"]


# Create fake demand so the academic project can demonstrate forecasting without real hospital data.
def generate_timeseries(number_of_days: int = 365) -> Path:
    """Generate one year of daily demand for all eight blood groups."""

    random_generator = np.random.default_rng(42)
    dates = pd.date_range("2025-01-01", periods=number_of_days, freq="D")
    rows = []

    # These different base values make the example look more like a real mixed inventory.
    base_demand = {
        "A+": 18,
        "A-": 5,
        "B+": 15,
        "B-": 4,
        "AB+": 7,
        "AB-": 2,
        "O+": 22,
        "O-": 6,
    }

    for blood_group in BLOOD_GROUPS:
        for day_number, date in enumerate(dates):
            # Weekends receive a small demand increase for this simple demonstration.
            weekend_spike = 1.15 if date.dayofweek >= 5 else 1.0
            # A gentle yearly wave adds a simple seasonal pattern to the synthetic data.
            seasonal_wave = 1 + 0.10 * np.sin(2 * np.pi * day_number / 365)
            random_noise = random_generator.normal(0, 2)
            demand = max(
                0,
                round(base_demand[blood_group] * weekend_spike * seasonal_wave + random_noise),
            )

            rows.append(
                {
                    "Date": date.strftime("%Y-%m-%d"),
                    "Blood_Group": blood_group,
                    "Demand": int(demand),
                }
            )

    dataset = pd.DataFrame(rows)
    output_path = Path(__file__).parent / "inventory_dataset.csv"
    dataset.to_csv(output_path, index=False)
    print(f"Created {len(dataset)} synthetic demand rows: {output_path}")
    return output_path


# Run the generator when this file is started directly from the command line.
if __name__ == "__main__":
    generate_timeseries()
