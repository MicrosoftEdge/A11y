import requests

jsonData = '''
{
	"id": "audio",
	"UIA": {
		"elements": [{
			"ControlType": "Group",
			"LocalizedControlType": "audio",
			"name": "Placeholder content",
			"children": [{
				"name": "Play"
			}, {
				"name": "Time elapsed/Skip back"
			}, {
				"name": "Seek"
			}, {
				"name": "Time remaining/Skip ahead"
			}, {
				"name": "Mute"
			}, {
				"name": "Volume"
			}]
		}]
	}
}
'''
r = requests.post('http://localhost:4119', jsonData)

print r.json()