# Unity-Simple-Property-Drawer
A property drawer base class that imitates Unity's custom inspectors but for property drawers.

I have always hated property drawers becuase they are overly complicated and lack the versitlity of Unity's custom inspectors. I got so tired of having to keep track of property rects and typing out verbose code that I made this simple class that simplifies property drawers and automatically keeps track of property heights and positions. It also includes some exta label options, spaces, and layout groups.

Inherit PropertDrawerbase as a base class instead of Unity's built in PropertyDrawer class to use.

<details>
  <summary>Methods</summary>
  
Return Type | Name | Description
------------|------|------------
GUIContent | BeginProperty( ref Rect aPosition, SerializedProperty aProperty, GUIContent aLabel ) | Call this at the beginning of the custom property.
void | EndProperty() | Call this at the end of the custom property.
void | DrawField( string aPropertyName ) | Draws a property field with the supplied property name using the base SerializedProperty.
void | DrawField( SerializedProperty aProperty, string aPropertyName ) | Draws a property field with the supplied name using the supplied SerializedProperty.
void | DrawLabel( string aString ) | Draws a label using the provided string.
void | DrawLabel( string aString, TextAnchor aAlignment ) | Draws a label using the provided string and text alignment.
void | DrawLabel( string aString, TextAnchor aAlignment, int aFontSize ) | Draws a label using the provided string, text alignment, and font size.
void | DrawSpace( [int aHeight] ) | Draws an empty space using the defualt space height or a specified height.
void | DrawFoldout( bool aFoldout, string aLabel, System.Action aDrawAction, out bool aIsOpen, [bool aToggleOnLabelClick = true] | Draws a foldout and it's content when opened.
void | AddSize( int aWidth, int aHeight ) | Call this whenever drawing a property field outside of DrawField() and DrawLabel() to calculate it's size into the drawer height and layout group dimensions.
void | BeginVertical() | Begins a vertial layout group.
void | EndVertical() | Ends a vertical layout group.
void | BeginHorizontal | Begins a horizontal layout group.
void | EndHorizontal | Ends a horizontal layout group.
void | SetFieldWidth( float aFieldWidth ) | Sets the width of fields. A value of less than 1 will result in the default field width.
void | SetFieldHeight( float aFieldHeight ) | Sets the height of fields. A value of less than 1 will result in the default field height.
float | GetHeight( SerializedProperty aProperty, string aPropertyName ) | Returns the height of a serialized property.
  
  </details>

<details>
  <summary>Example Useage</summary>

Example Class
```C#
[System.Serializable]
public class Example {
  public int myInt;
  public float myFloat;
  public string[] myStrings;
  
  public int groupedInt;
  public float groupedFloat;
  public int groupedString;
}
```

Property Drawer
```C#
using UnityEditor;

[CustomPropertyDrawer( typeof( Example ) )]
public class ExamplePropertyDrawer : PropertyDrawerBase {
  private bool propertyFoldout = true;
  
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

      BeginProperty( ref position, property, label );
      
      DrawLabel( "This is a label!" );
      DrawField( "myInt" );
      DrawField( "myFloat" );
      DrawField( "myStrings" );
      
      DrawSpace();
      
      DrawFoldout( propertyFoldout, "My Grouped Fields", () => {
          DrawField( "groupedInt" );
          DrawField( "groupedFloat" );
          DrawField( "groupedString" );
      }, out propertyFoldout );
      
      DrawSpace();
      
      DrawLabel( "Horizontal Layout", TextAnchor.MiddleCenter, 20 )
      BeginHorizontal();
      SetFieldWidth( 150 );
      DrawLabel( "Horizontal 1" );
      DrawLabel( "Horizontal 2" );
      DrawLabel( "Horizontal 3" );
      SetFieldWidth( 0 );
      EndHorizontal();

      EndProperty();
  }
}
```
  </details>
