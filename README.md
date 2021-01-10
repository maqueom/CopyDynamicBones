# CopyDynamicBones
A small utility to copy DynamicBones and colliders from one skeleton to another with the same hierarcy inside Unity.


## Author: Mario Maqueo
## Initial Release Date: January 10th, 2021
## Description: 
  Takes a source Transform and a destination Transform and traverses it copying all the dynamic bones present in the original, to the new one. Preserving the settings. 
  Can also be used for just getting stats like how many dynamic bones and dynamic bone colliders there are.
  (created originally to be able to reapply all dynamic bones from one model into a slightly modified version of the original one, for example after adjusting body proportions for use with full body tracking, as in VRChat)
             
## License: MIT License. (Copyright (c) 2021 Mario Maqueo) https://raw.githubusercontent.com/maqueom/CopyDynamicBones/main/LICENSE
    (Shoutouts/Attributions are appreciated! Thanks!)

Official website: https://github.com/maqueom/CopyDynamicBones

Unity version: 2018.4 and above (haven't tested it on earlier versions)

# Usage:   
   1. Add the CopyDynamicBones component to any object in a scene. (or place the DyamicBoneCopier prefab in the scene)
   2. Set the "source" to the root object of the original model.
   3. Set the "destination" to the root object of the new model.
   4. Right click the gear symbol (i.e. open the context menu) and click "Execute". ( or click "Get Stats" go get only stats without modifying anything)
         
         Note: This is meant to be executed OUTSIDE of play mode. (It will work inside play mode, but you will not be able to save your changes)
         Note 2: This code will not work correctly if the hierarchy of both models is different. (the objects inside must have the same names, as they are used when assigning the internal variables of the DynamicBone components like "root")
         
Important: The code will by default replace any DynamicBone and collider components already in the new model, so watch out. If you want to ignore preexisting dynamic bones, set the "Replace All DynamicBones" flag to false.
             For colliders you can set "Replace All Colliders" to false.
             Reference objects are assumed to be independent from the hierarchy and by default will not be changed. If you want to look for for a similar object inside the hierarchy then set the "Replace Reference Objects" flag to true
             
# About DynamicBone: 
DynamicBone is a Unity plugin created by Will Hong https://assetstore.unity.com/packages/tools/animation/dynamic-bone-16743 
                     It is used in software la VRChat for all sorts of animations like tails or bouncing boobs and other things.
                     
My plugin "CopyDynamicBones" needs DynamicBone to be present in the project to work. DynamicBone is a paid plug-in and not included here.

# Possibly common problems: 
If CopyDynamicBones is in a Plugins folder but Dynamic bone is not, then it will not work. Either both need to be inside Plugins (which might cause problems in VRChat for example), 
or both need to be outside Plugins. (You can have for example an OtherPlugins folder, and that one will be fine.)
                         

# Version history: 

1.0 Jan 10th 2021: Initial release

# Download:
Latest Release: [CopyDynamicBones v1.0 UnityPackage](https://github.com/maqueom/CopyDynamicBones/releases/download/v1.0/CopyDynamicBones1.0.unitypackage) 
