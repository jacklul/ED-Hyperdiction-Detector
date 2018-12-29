# Elite: Dangerous Hyperdiction Detector

This tool tries to predict the hyperdiction seconds before it happens.

**The general logic for identifying hyperdiction via journal file:**

- **StartJump** event is fired with **StarSystem** parameter set to your destination system
- **FSDTarget** event is fired with **Name** parameter with the value of **StarSystem** from previous event

**Example journal entries indicating hyperdiction:**

```
{ "timestamp":"2018-12-29T12:47:03Z", "event":"FSDJump", "StarSystem":"Ceti Sector OI-T b3-2", "SystemAddress":5068463613345, "StarPos":[-42.31250,-160.34375,-5.81250] }
{ "timestamp":"2018-12-29T12:47:34Z", "event":"StartJump", "JumpType":"Hyperspace", "StarSystem":"Kupol Vuh", "SystemAddress":3932008878802, "StarClass":"M" }
{ "timestamp":"2018-12-29T12:47:48Z", "event":"FSDTarget", "Name":"Kupol Vuh", "SystemAddress":3932008878802 }
{ "timestamp":"2018-12-29T12:48:01Z", "event":"FSDJump", "StarSystem":"Ceti Sector OI-T b3-2", "SystemAddress":5068463613345, "StarPos":[-42.31250,-160.34375,-5.81250] }
{ "timestamp":"2018-12-29T12:48:04Z", "event":"Music", "MusicTrack":"Unknown_Encounter" }
```
