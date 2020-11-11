# Master Race
This is one of my own projects, that I have made in my spare time. The game is about the evolution of mankind (based in the future). You, as the player got cloned and need to prove that you are capable of completing different tasks. These tasks consist of wallrunning, shooting, rocket-jumping, jumping, grappling and maintaining speed. The game is odly satisfying, when you have mastered all of the points mentioned above. This is because of the high skill ceiling. The player is basically able to use their skills to their full potential. Lastly the goal in the game is to complete the levels as fast and as badass as possible.

As you would expect, a good portion is made by myself. The only thing that I did not make is the art. I tried to make everything as loosely coppled as I could, to increase readability/reusability.

## Educational goals
The things that I wanted to learn from this project are:
- I wanted to learn wich steps I need to take to make a satisfying game. 
- I wanted to learn in's and outs of making a movement/physics based game.

## Scripts 
The scripts listed below are some scripts that I wanted to clarify, these scripts are also the most interesting in my opinion.

### Movement
[PlayerInput](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/PlayerInput.cs),
[PlayerMovement](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/PlayerMovement.cs),
[CrouchMovement](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/CrouchMovement.cs),
[WallrunMovement](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/WallrunMovement.cs)
The movement mechanics work thanks to a selection of scripts. This is, because I chose tho make the system use movement sockets. What I mean by this is that this system is easily expandable in movement mechanics. This also makes things very re-usable and a lot easier to read.
The first script is the PlayerInput script. This script handles the input from the player. This script then gives the input values to the PlayerMovement class. The PlayerMovement class is the brain of the whole system. This class handles the activation of the functions from the MovementSockets(CrouchMovement, WallrunMovement).
This is not all, the class also handles basic jumping, grounded checks, looking around. This is not all that special, so I am not going to expand on the comments. It is already straight forward. But the things that need some explanation are the sockets and the countermovement. I will start with the countermovement.
Countermovement is needed, because I am working with custom friction and physics based movement. Countermovement simply makes the player get to a stop relative to a variety of values. It basically adds negative force to the player, relative to the orientation of the player.

Now for the CrouchMovement class. This class handles the real funcitonality of crouching and sliding. When the StartCrouch function gets called from the PlayerMovement class, the player gets scaled down and boosted by a physical force relative to the orientation. When the player stops crouching, the player's scale gets reset.

The wallrunning of the game gets handled at the WallrunMovement class. To start wallrunning there must be a wall that is wallrunnable. So the CheckForWall function does so. It shoots Raycasts to the left and to the right. If these Raycasts do not collide with walls, the player stops wallrunning. If the player started wallrunning, the gravity gets temporarily disabled. This way the player is able to get stuck to the wall and does not fall off. There is also a force that gets added to the player, that makes the player stick to the wall extra well. Lastly, while the player is wallrunning the camera gets a smooth transition to an confortable angle. Because of this it looks like you are really wallrunning! 

### Weapon system
[ProjectileGun](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/weapon/ProjectileGun.cs), 
[CustomBullet](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/weapon/CustomBullet.cs), 
[PickupController](https://github.com/Royd54/Master-Race/blob/main/Assets/Player/Scripts/weapon/PickupController.cs)
Lets start with the ProjectileGun class. This class is very modular en reusable. This is because you can easily make tons of different styles of guns from the inspector. The class first starts to check for input. If there is a input for shooting, the gun shoots a Raycast. The ray gets shot from the camera's point of view. If the ray hit something the direction gets calculated between the attackpoint(muzzle) and the target. This direction then gets minipulated by the spread. After the final direction is set, the bullet gets spawned and shot with the calculated direction. Lastly the gun applies some recoil to the player in the form of a backwards force. The player can also reload the weapon. When the player reloads, the player is not able to shoot for x amount of seconds.

Next up is the CustomBullet class. This class checks for collisions and does different things relative to the colliders that it hits. A bullet can be an explosive, or a non-explosive. This is why it checks for objects around it with a overlapsphere, when it collides with something. Everything that is in range of the explosion gets effected by a explosionforce and takes damage. If the player is in range, the player gets blown back. This works, because the CounterMovementSetter function gets called. This function disables the grip of the player, so that the player slides back.

Last but not least is the PickupController class. This class handles the equipping of weapons. It first starts checking for weapons with a raycast that gets shot from the camera. If the player is already holding a gun, it makes the player drop the current weapon that they are holding. If the player is not holding a weapon, the player picks the weapon up. The layer of the object gets changes, so that the weapon does not clip through walls and gets rendered on top. The weapon then gets assigned to the correct position and gets parented to a gameobject (equipPos). After that the weapon gets enabled, so that the player is able to shoot! The player can also drop the weapon. Then the weapon gets their own values back and is not attached to the player anymore.

### Dialogue system
[Dialogue](https://github.com/Royd54/Master-Race/blob/main/Assets/DailogueWithAudio/Scripts/Dialogue.cs), 
[DialogueManager](https://github.com/Royd54/Master-Race/blob/main/Assets/DailogueWithAudio/Scripts/DialogueManager.cs), 
[DialogueTrigger](https://github.com/Royd54/Master-Race/blob/main/Assets/DailogueWithAudio/Scripts/DialogueTrigger.cs)
The dialogue class has 2 simple arrays, that contain sentences and audioclips for these sentences.
The dialoguemanager then works with these values. When a dialogue gets triggered the dialoguetrigger class starts the dialogue, and shuts down the collider/trigger. After this, the dialoguemanager start typing the sentence on a canvas letter for letter. While typing it also plays the first audioclip in the que. When the sentence and the audioclip are done, the next sentence and auioclip get played. If there is none of these left, the canvas will dissapear.

### GameEvents
[GameEvents](https://github.com/Royd54/Master-Race/blob/main/Assets/AI/Scripts/GameEvents.cs)
This class is a class that works with events/actions. This way everything stays loose, and easy to reuse.
You can say that this class is a serving hatch for the functions. 

### TakeCoverAI
[TakeCoverAI](https://github.com/Royd54/Master-Race/blob/main/Assets/AI/Scripts/TakeCoverAI.cs)
This is the script for the enemy AI in the game. This AI needs a navmesh to navigate itself. So if the navmesh/navmesh-agent is active/enabeled it starts checking the distance between the player en itself. If the distance to the player is too low, it starts to fall back. The AI can also fall back to a different coverspot if needed. If the player is not to close to the AI, it starts checking for cover. If there is any cover in range it runs towards cover and faces towards the player. The AI finds cover with a OverlapSphere. It then adds all the colliders, that are in the sphere and are on the coverlayer to an array. It then loops trough the array and checks wich spot is the closest. This spot gets assigned to a variable, and gets used by the TakeCover function. This fucntion makes the AI get into cover. After getting into cover the AI starts shooting at the player if they are in sight of the AI. The AI can also take damage and if it's health gets to 0 or below 0, it turns into a ragdoll.

### AudioManager
[AudioManager](https://github.com/Royd54/Space-Race/blob/master/Space%20Race/Assets/Player/Scripts/Sound/AudioManager.cs)
This script is for playing all kinds of audio. This script ties into the timescale of the engine. So if the engine goes in slomo, the music/sounds do aswell. Furthermore this script handles the audio in the game and is loosely made. You can simply use the instance of the script and call the functions.

## Sources
The sources that I used for this project are:
- [Countermovement](https://en.wikipedia.org/wiki/Countermovement)
- [Satisfaciton from games](http://www.gamesprecipice.com/satisfaction/)
