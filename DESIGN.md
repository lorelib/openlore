# Asset Loading
Godot generally likes files to be imported into the editor so they can be used. But that means that imported files are
then packaged into the files that are distributed, which is against the EULA.

An equivalent to Lantern Extractor is possible, but the user will need to run it on their copy of EQ, and it seems like
an extra hurdle.

There is a ResourceManager class that orchestrates all the loading, loaded assets go into 2 categories, global assets
and zone assets. Zone assets get wiped on switching zone. Ondemand assets will be considered zone assets.
