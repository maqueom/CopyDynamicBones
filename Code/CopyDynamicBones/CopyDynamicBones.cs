/*
 * Author: Mario Maqueo
 * Date: January 9th, 2021
 * Description: Takes a source Transform and a destination Transform and traverses it copying all the dynamic bones present in the original, to the new one. Preserving the settings. 
 *              Can also be used for just getting stats like how many dynamic bones and dynamic bone colliders there are.
 *              (created originally to be able to reapply all dynamic bones from one model into a slightly modified version of the original one, for example after adjusting body proportions for use with full body tracking, as in VRChat)
 *              
 * License: MIT License. (Copyright (c) 2021 Mario Maqueo) https://raw.githubusercontent.com/maqueom/CopyDynamicBones/main/LICENSE
 *          (Shoutouts/Attributions are appreciated! Thanks!)
 * 
 * Official website: https://github.com/maqueom/CopyDynamicBones
 * 
 * Unity version: 2018.4 and above (haven't tested it on earlier versions)
 * 
 * Usage:   1. Add the CopyDynamicBones component to any object in a scene. (or place the DyamicBoneCopier prefab in the scene)
 *          2. Set the "source" to the root object of the original model.
 *          3. Set the "destination" to the root object of the new model.
 *          4. Right click the gear symbol (i.e. open the context menu) and click "Execute". ( or click "Get Stats" go get only stats without modifying anything)
 *          
 *          Note: This is meant to be executed OUTSIDE of play mode. (It will work inside play mode, but you will not be able to save your changes)
 *          Note 2: This code will not work correctly if the hierarchy of both models is different. (the objects inside must have the same names, as they are used when assigning the internal variables of the DynamicBone components like "root")
 *          
 * Important: The code will by default replace any DynamicBone and collider components already in the new model, so watch out. If you want to ignore preexisting dynamic bones, set the "Replace All DynamicBones" flag to false.
 *              For colliders you can set "Replace All Colliders" to false.
 *              
 *              Reference objects are assumed to be independent from the hierarchy and by default will not be changed. If you want to look for for a similar object inside the hierarchy then set the "Replace Reference Objects" flag to true
 *              
 * About DynamicBone: DynamicBone is a Unity plugin created by Will Hong https://assetstore.unity.com/packages/tools/animation/dynamic-bone-16743 
 *                      It is used in software la VRChat for all sorts of animations like tails or bouncing boobs and other things.
 *                      
 *                      My plugin "CopyDynamicBones" needs DynamicBone to be present in the project to work. DynamicBone is a paid plug-in and not included here.
 * 
 * Possibly common problems: If CopyDynamicBones is in a Plugins folder but Dynamic bone is not, then it will not work. Either both need to be inside Plugins (which might cause problems in VRChat for example), 
 *                          or both need to be outside Plugins. (You can have for example an OtherPlugins folder, and that one will be fine.)
 *                          
 * 
 * Version history: 
 * 
 * 1.0 Jan 10th 2021: Initial release
 * 
 * */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CopyDynamicBones : MonoBehaviour
{
    public Transform source;
    public Transform destination;
    public bool replaceAllDynamicBones = true;
    
    public bool copyColliders = true;
    public bool replaceAllColliders = true;
    public bool replaceReferenceObjects = false;

    private int totalObjects, totalDynamicBones, totalDynamicBonesRemoved;
    private int totalColliders, totalCollidersRemoved;
    private int totalReferenceObjectsReplaced;

    private bool justGetStats = false;

    [ContextMenu("Execute")]
    void Execute()
    {
        if (source == null || destination == null)
        {
            Debug.LogError("[CopyDynamicBones]Error: source or destination transform is null.");
        }
        justGetStats = false;

        totalObjects = 0;
        totalDynamicBones = 0;
        totalDynamicBonesRemoved = 0;
        totalColliders = 0;
        totalCollidersRemoved = 0;
        totalReferenceObjectsReplaced = 0;

        if (copyColliders)
        {
            Debug.Log("[CopyDynamicBones]Copying all dynamic bone colliders from object " + source.name + " to object " + destination.name);
            TraverseTransformColliders(source, destination);
            Debug.Log("[CopyDynamicBones] A total of " + totalCollidersRemoved + " dynamic bone colliders were removed from destination hierarchy. And a total of " + totalColliders + " were copied.");
        }

        Debug.Log("[CopyDynamicBones]Copying all dynamic bones from object "+source.name+" to object "+destination.name);

        
        TraverseTransformDB(source, destination);

#if UNITY_EDITOR
        EditorUtility.SetDirty(destination);
#endif

        Debug.Log("[CopyDynamicBones]Finished. Traversed "+totalObjects+" total objects. A total of " + totalDynamicBonesRemoved + " were removed from destination hierarchy. And a total of " + totalDynamicBones + " were copied. "+totalReferenceObjectsReplaced+" reference objects were replaced.");
        
    }

    [ContextMenu("Get stats")]
    void GetStats()
    {
        if (source == null || destination == null)
        {
            Debug.LogError("[CopyDynamicBones]Error: source or destination transform is null.");
        }
        justGetStats = true;

        totalObjects = 0;
        totalDynamicBones = 0;
        totalDynamicBonesRemoved = 0;
        totalColliders = 0;
        totalCollidersRemoved = 0;
        totalReferenceObjectsReplaced = 0;

        Debug.Log("[CopyDynamicBones]Getting stats for source object " + source.name + " and destination object: "+destination.name);

        if (copyColliders)
        {
            
            TraverseTransformColliders(source, destination);
            Debug.Log("[CopyDynamicBones]STATS: A total of " + totalCollidersRemoved + " dynamic bone colliders were found in the destination hierarchy. And a total of " + totalColliders + " were found in the source hierarchy.");
        }
        TraverseTransformDB(source, destination);

        Debug.Log("[CopyDynamicBones]Finished STATS. Traversed " + totalObjects + " total objects. A total of " + totalDynamicBonesRemoved + " dynamic bones were found in the destination hierarchy. And a total of " + totalDynamicBones + " were found in the source hierarchy. " + totalReferenceObjectsReplaced + " reference objects were found marked to be replaced.");

    }

    void ProcessDynamicBones(Transform sourceTransform, Transform destTransform)
    {
        DynamicBone[] sourceDBs = sourceTransform.GetComponents<DynamicBone>();

        DynamicBone[] destDBs = destTransform.GetComponents<DynamicBone>();

        //Remove all preexisting dynamic bones in the destination object
        if (replaceAllDynamicBones)
        {
            foreach (DynamicBone bone in destDBs)
            {
                if (bone == null) continue;
                if (!justGetStats)
                {
                    Debug.Log("[CopyDynamicBones]Removing DynamicBone in " + bone.name);
                    DestroyImmediate(bone);
                }
                this.totalDynamicBonesRemoved++;
            }
        }

        //Copy all bones found in the original object to the destination
        foreach (DynamicBone sourceBone in sourceDBs)
        {
            DynamicBone destBone = null;
            if (!justGetStats)
            {
                destBone = destTransform.gameObject.AddComponent<DynamicBone>();
                Debug.Log("[CopyDynamicBones]Copying DynamicBone from " + sourceBone.name + " to: " + destBone.name);
                CopyValues(sourceBone, destBone);

                //Assign variables that might have references
                //First the "Root".
                if (sourceBone.m_Root != null)
                {
                    destBone.m_Root = GetDestinationEquivalent(sourceBone.m_Root);
                }

                //Then the colliders list
                if (sourceBone.m_Colliders != null)
                {
                    List<DynamicBoneColliderBase> collidersList = new List<DynamicBoneColliderBase>();
                    foreach (DynamicBoneColliderBase sourceCollider in sourceBone.m_Colliders)
                    {
                        DynamicBoneColliderBase newCollider = GetDestinationEquivalent(sourceCollider.transform).GetComponent<DynamicBoneColliderBase>();
                        collidersList.Add(newCollider);
                    }
                    destBone.m_Colliders = collidersList;
                }

                //Then the exclusions list
                if (sourceBone.m_Exclusions != null)
                {
                    List<Transform> exclusionsList = new List<Transform>();
                    foreach (Transform excludedT in sourceBone.m_Exclusions)
                    {
                        Transform newExcludedT = GetDestinationEquivalent(excludedT.transform);
                        exclusionsList.Add(newExcludedT);
                    }
                    destBone.m_Exclusions = exclusionsList;
                }
            }
            //And finally the ReferenceObject.
            if (replaceReferenceObjects && sourceBone.m_ReferenceObject != null)
            {
                if (!justGetStats)
                {
                    destBone.m_ReferenceObject = GetDestinationEquivalent(sourceBone.m_ReferenceObject);
                }

                totalReferenceObjectsReplaced++;
            }


            this.totalDynamicBones++;
        }
    }

    void TraverseTransformDB(Transform sourceTransform, Transform destTransform)
    {
        ProcessDynamicBones(sourceTransform, destTransform);

        for (int i = 0; i < sourceTransform.childCount; i++)
        {
            this.totalObjects++;

            Transform thisChildT = sourceTransform.GetChild(i);
            Transform thisChildD = destTransform.GetChild(i);
            

            TraverseTransformDB(thisChildT, thisChildD);
        }

    }

    void ProcessColliders(Transform sourceTransform, Transform destTransform)
    {
        DynamicBoneCollider[] sourceDBCs = sourceTransform.GetComponents<DynamicBoneCollider>();

        DynamicBoneCollider[] destDBCs = destTransform.GetComponents<DynamicBoneCollider>();

        //Remove all preexisting dynamic bone colliders in the destination object
        if (replaceAllColliders)
        {
            foreach (DynamicBoneCollider collider in destDBCs)
            {
                if (collider == null) continue;
                if (!justGetStats)
                {
                    Debug.Log("[CopyDynamicBones]Removing DynamicBoneCollider in " + collider.name);
                    DestroyImmediate(collider);
                }

                this.totalCollidersRemoved++;
            }
        }

        //Copy all dynamic bone colliders found in the original object to the destination
        foreach (DynamicBoneColliderBase sourceBC in sourceDBCs)
        {
            if (!justGetStats)
            {
                DynamicBoneCollider destBC = destTransform.gameObject.AddComponent<DynamicBoneCollider>();
                Debug.Log("[CopyDynamicBones]Copying DynamicBoneCollider from " + sourceBC.name + " to: " + destBC.name);
                CopyValues(sourceBC, destBC);
            }

            this.totalColliders++;
        }
    }

    void TraverseTransformColliders(Transform sourceTransform, Transform destTransform)
    {
        ProcessColliders(sourceTransform, destTransform);

        for (int i = 0; i < sourceTransform.childCount; i++)
        {
            this.totalObjects++;

            Transform thisChildT = sourceTransform.GetChild(i);
            Transform thisChildD = destTransform.GetChild(i);
            

            TraverseTransformColliders(thisChildT, thisChildD);
        }

    }

    /// <summary>
    /// Takes a transform inside the source hierarchy and gets the equivalent in the destination hierarchy
    /// (used to assign the "root" variable in the new DynamicBone component, and similar cases)
    /// </summary>
    /// <param name="sourceTransform"></param>
    /// <returns></returns>
    Transform GetDestinationEquivalent(Transform sourceTransform)
    {
        return FindTransformWithNameInChilds(destination, sourceTransform.name);

    }

    Transform FindTransformWithNameInChilds(Transform t, string name)
    {
        if (t.name == name) return t;

        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
    
            Transform childResult = FindTransformWithNameInChilds(child, name);
            if (childResult != null) return childResult;

        }
        return null;
    }

    void CopyValues<T>(T from, T to)
    {
        var json = JsonUtility.ToJson(from);
        JsonUtility.FromJsonOverwrite(json, to);
    }
}
