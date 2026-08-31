"""Create a small synthetic donor-response dataset for the academic prototype."""

from pathlib import Path

import numpy as np
import pandas as pd


# Generate fake data so the prototype can be demonstrated without exposing real donor data.
def generate_dataset(number_of_rows: int = 1000) -> Path:
    """Create 1,000 example donors and save them beside this script."""

    # A fixed seed makes the demo produce the same data each time it is run.
    random_generator = np.random.default_rng(42)

    donor_ids = np.arange(1, number_of_rows + 1)
    distance_km = random_generator.uniform(1, 100, number_of_rows).round(2)
    requests_received = random_generator.integers(0, 21, number_of_rows)

    # Accepted requests are generated from received requests so the values stay realistic.
    acceptance_chance = random_generator.uniform(0.25, 0.85, number_of_rows)
    requests_accepted = np.array(
        [
            random_generator.binomial(received, chance)
            for received, chance in zip(requests_received, acceptance_chance)
        ]
    )
    gamification_xp = random_generator.integers(0, 1001, number_of_rows)

    # A donor is more likely to respond when they are nearby, have more XP, and accepted requests before.
    response_rate = np.divide(
        requests_accepted,
        requests_received,
        out=np.zeros(number_of_rows, dtype=float),
        where=requests_received != 0,
    )
    response_probability = (
        0.25
        + 0.30 * (1 - distance_km / 100)
        + 0.25 * (gamification_xp / 1000)
        + 0.20 * response_rate
    )
    response_probability = np.clip(response_probability, 0.05, 0.95)
    will_respond = random_generator.binomial(1, response_probability)

    dataset = pd.DataFrame(
        {
            "Donor_Id": donor_ids,
            "Distance_KM": distance_km,
            "Requests_Received": requests_received,
            "Requests_Accepted": requests_accepted,
            "Gamification_XP": gamification_xp,
            "Will_Respond": will_respond,
        }
    )

    output_path = Path(__file__).parent / "donor_dataset.csv"
    dataset.to_csv(output_path, index=False)
    print(f"Created synthetic dataset with {number_of_rows} rows: {output_path}")
    return output_path


# Run the generator when the file is started directly from the command line.
if __name__ == "__main__":
    generate_dataset()
