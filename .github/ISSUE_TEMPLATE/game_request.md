---
name: Game Request
about: Request support for a new game or application.
labels: 'Type: Enhancement,Area: Game Support'
---
<!--
Please make sure you SEARCH for any other issues requesting this game before submitting your issue - it might already have been requested.

If you haven't found a similar issue, please carry on. Make sure that when you fill in the responses you delete the square brackets as comments inside these are not visible in the bug report.

To be able to add game state support for a particular game we need a way of reading the values from the game. This can be done in one of several ways:

  1. Sometimes the game itself makes the relevant variables available (e.g. CSGO, Dota 2). This is quite uncommon and your requested game probably won't do this.
  
  2. Another option is mod support. Depending on the level of support, we may be able to make a custom mod that gets useful variables and sends them (via HTTP request) to Aurora (e.g. Minecraft, Subnautica).

  3. We may be able to read values from the game itself using memory reading. This WILL NOT BE DONE ON MULTIPLAYER GAMES or games with anti-cheat as memory reading can be used by cheats to give players an advantage, and so many anti-cheat systems will flag you if memory reading is detected.

  4. If the game already has support for lighting (e.g. it natively supports Razer Chroma) you may be able to patch the game and use the wrapper layer on Aurora to get the values from the existing system. This does not always work as sometimes games will not load modified DLLs. For this to work, we do not need to add anything special and you do not need to make an issue.
  
We are continuing to look at other options of getting data from games (such as optical character recgonition). Sadly if your game does not fit into any of the above groups, then Aurora cannot currently support your requested game.

If it does, feel free to fill in the following report and submit the issue.

-->

Please add support for the following game:

**Game Name:** <!-- What game is it? -->

**Methods:** <!-- Please tell us which methods of getting data this game has (see above). Native GSI/Mod support/Memory reading -->
