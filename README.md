# Multiplayer Battle Sample
Made by Ilyas Kharisov a.k.a. Jack Lumers.

A sample game with local multiplayer. Getting myself knowing Mirror Networking.

# Project Setup
1. Open in Unity 2021.3.24f1 (not tested on other versions but probably will work fine);
2. Game -> Scenes -> BattleScene
3. Enter playmode, chose whatever you host or connect to existing room (using local network IP)
3. Run built game exe, chose whatever you host or connect to existing room.
4. Also in order to host/connect, you can run another game instance instead of using playmode.

# Building your own version
No any special preparations, but I build using configuration as follows:
1. Mono scripting backend.
2. Windows standalone platform. 

Not tested on others settings.

# Configuring
1. You can configure round restart timing and max score in **BattleConfig**. Round restart timing defines delay between rounds. Max score is used to define how many times player need to dash in another player.
Therer also colors array that is used to randomly set color to players.
3. **DummyPlayerMetadataConfig** is used to setup invincibility timings, dash power, setting initial values for some player flags, movement configuration.

# Features
* Local multiplayer;
* Gamepad support;
* Dash power configuration;
* Invincibility timing configuration;
* Max score for round configuration;
* Round start timer configuration;

# Used assets/packages
* Mirror Networking: https://mirror-networking.com/
* DoTween: http://dotween.demigiant.com/
* UniTask: https://github.com/Cysharp/UniTask
* Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/index.html
* Cinemachine: https://docs.unity3d.com/Packages/com.unity.cinemachine@2.3/manual/index.html

# Possible problems
Not tested for cheating possibilities. Not tested for bad network, like high ping or packet loss.

