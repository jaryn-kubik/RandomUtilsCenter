import yt_dlp

def Init(logger):
	global ydl
	ydl = yt_dlp.YoutubeDL({
		"outtmpl": "D:\\Downloads\\%(title)s.%(ext)s",
		"logger": logger
	})
	return True

def GetExtractor(url):
	for ie_key, ie in ydl._ies.items():
		if ie.suitable(url):
			return ie_key
	return "Generic"

def Download(url):
	return ydl.download([url])