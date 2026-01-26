# Spotify Song Request Manager

## Pages and Features

### Request Songs
* Anyone can search up songs
* When a song is clicked on, it is requested
* Anyone can request multiple songs upto the Request Limit as set in settings (Default)
* Anyone can request multiple of the same song, unless disabled in settings
* Anyone can view previously requested songs
  * Cliking on these can also request songs
*  If a user is on mobile, there is a navigation bar to navigate between Request Songs and Queue
*  Long press/Shfit + Click/Ctrl + Click/Logged in as admin + Click of the logo opens up a navigation menu to all pages

### Queue
* anyone can view the currently playing and queue of the master admin
  * Master admin is set in settings
  * This is meant for continous play. It does not detect if a song gets paused and only updates when it should have go to the next song.
*  If a user is on mobile, there is a navigation bar to navigate between Request Songs and Queue
*  Long press/Shfit + Click/Ctrl + Click/Logged in as admin + Click of the logo opens up a navigation menu to all pages

### Song Manager
* Includes all features of the Request Songs Page except the mobile navigation bar
* Admin can add songs to a playlist
  * Uses the master admin's account to add to the playlist
  * must be logged in to complete
  * playlist is selected in settings
  * automatically adds to the end of the playlist
    * if you want the most popular song to last, you must add it last
* Click the logo to open up a navigation menu to all pages

### Settings
* Login
  * Required to change any settings
* Master Admin
  * display's this admin's currently playing and queue
  * uses this account to add songs to a playlist
    * must be a colaborator of the playlist
* Playlist selector
  * This is the playlst that you can add songs to with the Song Manager Page
* Song Request Limit
  * This is the max number of songs a person can request
* Allow Repeats
  * This toggles if a person can request the same song multiple times
* Auto Add
  * This automatically adds songs to the selected playlist
  * Quantity
    * This is the number of songs that gets automatically added to the playlist
  * Set Time
    * Select the day(s) and time you want the songs to be added
* Clear Requests
  * Clears all requets made
 
## Features built in but not accessable
### Blacklist
* Songs in this list don't show up when you search for them

### Highligted Songs (Line Dances)
* Songs in this list are highlighted on the queue
