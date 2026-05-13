using UnityEngine;
using UnityEditor;

public class RemoveArmature
{
    [MenuItem("Tools/Remove Armature Prefix")]
    static void RemovePrefix()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;

        if (clip == null)
        {
            Debug.LogError("Selecciona un AnimationClip");
            return;
        }

        var bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);

            var newBinding = binding;
            newBinding.path = binding.path.Replace("Armature/", "");

            AnimationUtility.SetEditorCurve(clip, binding, null);
            AnimationUtility.SetEditorCurve(clip, newBinding, curve);
        }

        Debug.Log("Se removió Armature/ de los paths");
    }
}