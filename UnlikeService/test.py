import requests

LOGIN_URL = "http://52.203.72.116:8080/login"
EMAIL = "allan"          
PASSWORD = "1234"                        

UNLIKE_URL = "http://3.222.208.200:8080/unlike"
ID_PUBLICATION = "685b517f51f100fc498329ed" 

login_data = {
    "User_mail": EMAIL,
    "password": PASSWORD
}   

print("login...")
login_response = requests.post(LOGIN_URL, json=login_data)

if login_response.status_code != 200:
    print("Error to login:", login_response.status_code)
    print(login_response.text)
    exit()

token = login_response.json().get("token")
if not token:
    print("issue getting token")
    print(login_response.json())
    exit()

print("Token getted successfully:")

headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

unlike_data = {
    "IdPublication": ID_PUBLICATION
}

print("Sending unlike request")
response = requests.post(UNLIKE_URL, json=unlike_data, headers=headers)

print("Response:", response.status_code)

try:
    print("Response JSON:")
    print(response.json())
except Exception:
    print("Not have response in json format :")
    print(response.text)

