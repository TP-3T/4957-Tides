using System;
using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.Managers;
using TTT.ModularData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

/*
* If tests results require frames to update, use the UnityTest attribute and yield return null to skip a frame.
* Example: Flooding changes terrain over multiple frames, so tests related to flooding should use UnityTest.
* Nothing here is final and can be changed as needed.
* It's very likely anything that requires RPCs or networking will need to be moved to PlayModeTests.
*/
public class EditModeTests
{

}
