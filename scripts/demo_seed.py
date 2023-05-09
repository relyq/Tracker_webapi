import sys
import requests
import json
import os

base_url_local='https://localhost:7004'
base_url_pve='https://relyq.silics.com:7004'
base_url_aws='https://aws-tracker-api.relyq.dev:7004'
base_url=os.environ.get('Tracker__BaseUrl')
if not base_url:
    sys.exit("must provide base url")
org_id = sys.argv[1]
if not org_id:
    sys.exit("must provide org id")
demo_user_email='DemoAdmin@tracker.silics.com'

def login(email, password):
  headers = {
      'Content-Type': 'application/json',
  }

  json_data = {
      'email': email,
      'password': password,
  }

  return requests.post(f'{base_url}/api/Auth/login', headers=headers, json=json_data)

def login_demo(org_id):
  headers = {
      'Content-Type': 'application/json',
  }

  json_data = f'{org_id}'

  return requests.post(f'{base_url}/api/Auth/login/demo', headers=headers, json=json_data)


def get_user(jwt, email):
  headers = {
      'Authorization': f'Bearer {jwt}',
  }

  return requests.get(f'{base_url}/api/Users/email/{email}', headers=headers)

def get_projects(jwt):
  headers = {
      'Authorization': f'Bearer {jwt}',
  }

  return requests.get(f'{base_url}/api/Projects', headers=headers)

def delete_project(jwt, project_id):
  headers = {
    'Authorization': f'Bearer {jwt}',
  }
  return requests.delete(f'{base_url}/api/Projects/{project_id}', headers=headers)

def post_project(jwt, user_id, name, description):
  headers = {
      'Authorization': f'Bearer {jwt}',
      'Content-Type': 'application/json',
  }

  json_data = {
      'id': 0,
      'authorId': user_id,
      'name': name,
      'description': description,
      'created': '2022-11-10T16:02:45.398Z',
  }

  return requests.post(f'{base_url}/api/Projects', headers=headers, json=json_data)

def post_ticket(jwt, project_id, title, description, priority, ttype, status, user_id):
  headers = {
      'Authorization': f'Bearer {jwt}',
      'Content-Type': 'application/json',
  }

  json_data = {
      'id': 0,
      'projectId': project_id,
      'title': title,
      'description': description,
      'priority': priority,
      'type': ttype,
      'status': status,
      'submitterId': user_id,
      'assigneeId': '',
      'created': '2022-11-10T16:02:45.399Z',
      'activity': '2022-11-10T16:02:45.399Z',
      'closed': '2022-11-10T16:02:45.399Z',
  }

  return requests.post(f'{base_url}/api/Tickets', headers=headers, json=json_data)

def post_comment(jwt, ticket_id, content):
  headers = {
      'Authorization': f'Bearer {jwt}',
      'Content-Type': 'application/json',
  }
  
  json_data = {
      'id': 0,
      'ticketId': ticket_id,
      'authorId': '',
      'content': content,
      'created': '2022-11-10T16:02:45.399Z'
  }

  return requests.post(f'{base_url}/api/Comments', headers=headers, json=json_data)


# login

response = login_demo(org_id)

res = json.loads(response.content)

jwt = res["jwt"]

# get demo user id
response = get_user(jwt, demo_user_email)

res = json.loads(response.content)

demo_user_id = res["id"]

"""
# get projects
response = get_projects(jwt)

res = json.loads(response.content)

projects_count = res["count"]
projects = res["projects"]

# delete projects
for project in projects:
  delete_project(jwt, project["id"])
"""

# seed projects
response = post_project(jwt, demo_user_id, "Getting started", "Features guide project")

res = json.loads(response.content)

new_project = res

# seed tickets
response = post_ticket(jwt, new_project["id"], "This is a ticket", "This is the ticket's description.", 3, "issue", "open", demo_user_id)

res = json.loads(response.content)

new_ticket = res

# seed comments
response = post_comment(jwt, new_ticket["id"], "This is a comment.")
