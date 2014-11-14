EmbeeEDNavComm
==============

A VoiceAttack plugin for Elite: Dangerous 

Utilises a C# VoiceAttack (VA) plugin (http://voiceattack.com) DLL, that in turn communicates with a C# system tray application via named pipes to calculate a route between star systems in the Elite:Dangerous game (elitedangerous.com).

Relies heavily on the Elite:Dangerous Star Coordinator (EDSC) API [http://www.edstarcoordinator.com/] to retrieve star system name and coordinates.

So far the plugin accepts current system, target system, and jump range parameters, and returns information on which system you should jump to next, or error messages if no routes have been found, or information is missing.

Examples of proposed usage (made up route, not accurate):
```
> "current system Eranin"
< "currently in Eranin"
> "jump range 8.0"
< "jump range set to 8.0"
> "target system CE Bootis"
< "course set for CE Bootis. That will take 7 jumps. First jump is Aulin"
> "I'm in Aulin"
< "next jump is Styx - 6 jumps to go until CE Bootis"
```
etc


##Still to do before it's usable:
* Make a VA command template for users to import, and actually test it using VoiceAttack

##Features to add:
* Ability to exclude systems (i.e. anarchy systems, or systems without a base to refuel at)
* Ability to ask for closest system of type X (ie closest High Tech system, or closest Federation system)
* Integration with an E:D trading tool to ask for best place to sell commodity X
* Ability to save commodity information ("this system has a high demand for Auto Fabricators")
