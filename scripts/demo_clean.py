import sys
import requests
import json
import os

#base_url_vps='https://vps-2933482-x.dattaweb.com:7004'
base_url_local='https://localhost:7004'
base_url_pve='https://relyq.silics.com:7004'
base_url_aws='https://aws-tracker-api.relyq.dev:7004'
base_url=os.environ.get('Tracker__BaseUrl')
if not base_url:
    sys.exit("must provide base url")
demo_user_id='83828f8c-550f-4551-940d-72035374f95e'
demo_org_id='9b001357-c03f-4cf5-9648-44fba3491745'
janitor_email='janitor@tracker.silics.com'
janitor_password=os.environ.get('Secrets__JanitorPassword')

def login(email, password):
  headers = {
      'Content-Type': 'application/json',
  }

  json_data = {
      'email': email,
      'password': password,
  }

  return requests.post(f'{base_url}/api/Auth/login', headers=headers, json=json_data)

def delete_org(jwt, org_id):
  headers = {
    'Authorization': f'Bearer {jwt}',
  }
  return requests.delete(f'{base_url}/api/Organizations/{org_id}', headers=headers)

def get_user(jwt, id):
    headers = {
        'Authorization': f'Bearer {jwt}',
    }

    return requests.get(f'{base_url}/api/Users/{id}', headers=headers)

# login
response = login(janitor_email, janitor_password)

res = json.loads(response.content)

jwt = res["jwt"]

# get orgs
response = get_user(jwt, demo_user_id)

res = json.loads(response.content)

#org_count = res["count"]
orgs = res["organizationsId"]
orgs.remove(demo_org_id)

# delete orgs
for org in orgs:
  delete_org(jwt, org)
