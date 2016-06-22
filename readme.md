## The Phoenix Protocol

![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/ss.png)

#### Introduction

The Phoenix Protocol is an epic space-fantasy video game where players form a team to survive an onslaught of hostile alien ships as they escape from the fallen Earth. It is a cooperative action-strategy hybrid that encourages players to communicate with one another to decide how best to make use of their abilities and aid their survival. The game is set in a room which forms the bridge of a spaceship with three large back projected screens that surround the players and immerse them into the universe.

The project began as a third year computer science group module, with significant focus on solving technical challenges and delivering an immersive experience. The team consisted of 7 developers (Dillon Diep, Marc Steene, Artur Gemes, Andrei Poenaru, Stoil Ganev, Frank Hemsworth, and Luke Bryant). The project was mentored by Andrew Stubbs, and music composition was completed by Tom Vos, Eleftherios Chrysanthou, and George R. Gaitanos.

[![ScreenShot](http://www.allprodad.com/wp-content/uploads/2014/06/video_default.png)](https://www.facebook.com/ArchDD/videos/10154133445978537/)

#### Local Testing

#### Launching

While the gaming experience was optimised for a LAN environment with infra-red motion controls to input shooting via wiimotes, it is possible to build the source on Unity Engine and play locally with multiple clients on a single machine.

When launching the game, a server must first be started via the 'Create Game' button.
![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/game_first_screen.png)

Clients can then be added as networked cameras (for extending view in a parallel and sensible manner for wide field of view), engineer drone, or as the commander console where upgrades and repairs can be issued for the player ship.

![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/game_lobby_screen.png)

As clients join the game, tokens are populated in the lobby scene. These tokens may be dragged to a specific role to reassign the client.

When ready, press start to launch the game. Upon clicking the start button in the lobby, a tutorial diagram is displayed. Press enter once all players are ready to start.

#### Basic Controls
##### Main Ship Controls
###### Mouse Input
As the game was intended to be played with wiimotes, press C to enable mouse aiming, with mouse aiming enabled:
* Mouse movement to aim
* Left mouse button to shoot

It is only possible to shoot on the main screen (server) without wiimotes.

###### Navigation
WASD or Up,Down,Left,Right to move

##### Others
The engineer client follows similar input methods, the command console is a point and click or touch-based UI.

#### Features

##### Multi-Role
Player ship (this would just be multiple camera clients in our designated setup room, but movement is done through the main camera host when testing locally)
![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/cam.jpg)
The engineer drone is a smaller ship that moves around the main ship to perform upgrades and repairs issued by the commander
![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/eng.jpg)
The commander issues upgrades and repairs for the engineer to perform, and use special abilities with cooldowns
![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/command.jpg)
There is also a phone game ran on a separate server for spectators to hack enemy ships and control them using their mobile devices during gameplay.

##### Networked Aligned Cameras
Increasing field of view too much on a single camera would cause distorted view. The solution provides aligned cameras that are networked to split the view and render in parallel for maximal performance.
![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/frustum.png)
The alignment of cameras is achieved via working out the camera frustum vectors and aligning the left vector to the right. It is also possible to extend vertically. Below is an example of 5 cameras aligned, there is no hard limit - the user may go beyond 360 degree view should they wish.
![alt tag](https://github.com/ArchDD/The-Phoenix-Protocol/blob/master/report/images/cameras.jpg)

##### Infrared motion point shooting
Unfortunately this feature requires a full set up with additional hardware such as the IR LEDs and correct data relay.

##### Limitations
While the project was developed to avoid dependencies for ease of testing and flexibility, the true experience requires a setup similar to the video shown above.
