using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer (typeof (UpgradeProperties))]
public class UpgradePropertiesDrawer : PropertyDrawer {

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);
        EditorGUI.indentLevel = 0;
        //EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("name"), GUIContent.none);


        EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y, 100, 20), "Name: ");
        EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y, 200, 20), property.FindPropertyRelative ("name"), GUIContent.none);

        EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 25, 100, 20), "Type: ");
        EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 25, 200, 20), property.FindPropertyRelative ("type"), GUIContent.none);

        EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 50, 100, 40), "Description: ");
        EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 50, 200, 40), property.FindPropertyRelative ("description"), GUIContent.none);

        EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 100, 100, 20), "Cost: ");
        EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 100, 200, 20), property.FindPropertyRelative ("cost"), GUIContent.none);

        EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 120, 100, 20), "Levels: ");
        EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 120, 200, 20), property.FindPropertyRelative ("numberOfLevels"), GUIContent.none);

        EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 140, 100, 20), "Repairable: ");
        EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 140, 200, 20), property.FindPropertyRelative ("repairable"), GUIContent.none);


        switch(property.FindPropertyRelative("type").intValue)
        {
            case (int)ComponentType.ShieldGenerator:
                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 180, 100, 20), "Max Shield Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 180, 200, 20), property.FindPropertyRelative ("shieldsMaxShieldUpgradeRate"), GUIContent.none);

                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 200, 100, 20), "Max Recharge Rate Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 200, 200, 20), property.FindPropertyRelative ("shieldsMaxRechargeRateUpgradeRate"), GUIContent.none);
                break;
            case (int)ComponentType.Turret:
                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 180, 100, 20), "Turrets Max Damage Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 180, 200, 20), property.FindPropertyRelative ("turretsMaxDamageUpgradeRate"), GUIContent.none);

                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 200, 100, 20), "Turrets Min Fire Delay Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 200, 200, 20), property.FindPropertyRelative ("turretsMinFireDelayUpgradeRate"), GUIContent.none);
                break;
            case (int)ComponentType.Engine:
                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 180, 100, 20), "Engine Max Speed Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 180, 200, 20), property.FindPropertyRelative ("engineMaxSpeedUpgradeRate"), GUIContent.none);

                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 200, 100, 20), "Engine Max Turning Speed Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 200, 200, 20), property.FindPropertyRelative ("engineMaxTurningSpeedUpgradeRate"), GUIContent.none);
                break;
            case (int)ComponentType.Hull:
                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 180, 100, 20), "Hull Max Health Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 180, 200, 20), property.FindPropertyRelative ("hullMaxHealthUpgradeRate"), GUIContent.none);

                break;
            case (int)ComponentType.Drone:
                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 180, 100, 20), "Drone Movement Speed Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 180, 200, 20), property.FindPropertyRelative ("droneMovementSpeedUpgradeRate"), GUIContent.none);

                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 200, 100, 20), "Drone Improvement Time Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 200, 200, 20), property.FindPropertyRelative ("droneImprovementTimeUpgradeRate"), GUIContent.none);

                break;
            case (int)ComponentType.ResourceStorage:
                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 180, 100, 20), "Storage Collection Bonus Upgrade Rate: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 180, 200, 20), property.FindPropertyRelative ("storageCollectionBonusUpgradeRate"), GUIContent.none);

                EditorGUI.LabelField(new Rect (contentPosition.x, contentPosition.y + 200, 100, 20), "Storage Interest Rate Upgrade Bonus: ");
                EditorGUI.PropertyField (new Rect (contentPosition.x + 100, contentPosition.y + 200, 200, 20), property.FindPropertyRelative ("storageInterestRateUpgradeBonus"), GUIContent.none);

                break;
            case (int)ComponentType.None:
                break;
        }
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        return 300f;
    }
}