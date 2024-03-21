import csv
import gzip
import itertools
import random
from datetime import datetime
from datetime import timedelta

def generate_random_supermarket_data(num_items):
    items = []
    for _ in range(num_items):
        item_id = random.randint(1, 2**63 - 1)  # Generate a random 64-bit integer
        price = round(random.uniform(0.5, 50.0), 2)
        items.append({'ItemID': item_id, 'Price': price})
    return items

def generate_random_users(num_items):
    items = []
    for _ in range(num_items):
        user_id = random.randint(100_000_000, 100_000_000 * 389)
        items.append(user_id)
    return items

def generate_random_qty():
    choice = random.uniform(0, 1)
    
    if choice < 0.5:
        return random.randint(1, 1)  # 50% chance of getting 1 item
    elif choice < 0.75:
        return random.randint(1, 2)  # 25% chance of getting 2 items
    elif choice < 0.85:
        return random.randint(1, 3)  # 10% chance of getting 3 items
    elif choice < 0.95:
        return random.randint(1, 7)  # 10% chance of getting up to 7 items
    else:
        return random.randint(1, 100)  # 5% chance of getting any number from 1 to 100

def get_random_weighted_item():
    choice = random.uniform(0, 1)
    
    if choice < 0.5:
        return random.choice(ITEMS[:15])  # 50% chance of getting top 15 items
    elif choice < 0.75:
        return random.choice(ITEMS[5:150])  # 25% chance 
    elif choice < 0.85:
        return random.choice(ITEMS[100:500])  # 10% chance of getting 3 items
    elif choice < 0.95:
        return random.choice(ITEMS[300:700])  # 10% chance of getting up to 7 items
    else:
        return random.choice(ITEMS[450:])  # 5% chance of getting any item from the list

    
def gen_point_of_sale_data():
    last = datetime.now()
    while True:
        user_id = random.choice(USERS)
        date = last
        last = last + timedelta(seconds=random.randint(1, 60))
        for _ in range(random.randint(1, 12)):
            item = get_random_weighted_item()
            quantity = generate_random_qty()
          
            record = {
                'UserID': user_id,  
                'ItemID': item['ItemID'],
                'Quantity': quantity,
                'Price': item['Price'],
                'Date': date,
            }
            yield record
        

def write_to_gzip_csv(number, filename):
    with gzip.open(filename, 'wt', newline='\n', encoding='utf-8') as file:
        fieldnames = ['UserID', 'ItemID', 'Quantity', 'Price', 'Date']
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()
        for row in itertools.islice(gen_point_of_sale_data(), number):
            writer.writerow(row)


random.seed(2023_12_24)

ITEMS = generate_random_supermarket_data(5_000)
USERS = generate_random_users(10_000_000)


if __name__ == "__main__":
    write_to_gzip_csv(20_000_000, 'data.csv.gz')
        