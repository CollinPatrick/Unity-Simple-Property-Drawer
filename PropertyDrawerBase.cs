using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PropertyDrawerBase : PropertyDrawer {
    private class LayoutDirectionRect {
        private float _x;
        private float _y;
        private float _height = 0;
        private float _width = 0;
        private LayoutDirection _direction;

        public float x => _x;
        public float y => _y;
        public float height => _height;
        public float width => _width;
        public LayoutDirection direction => _direction;

        public LayoutDirectionRect( float aXPos, float aYPos, LayoutDirection aDirection ) {
            _x = aXPos;
            _y = aYPos;
            _direction = aDirection;
        }

        public void AdjustHeight( float aHeight ) {
            if ( direction == LayoutDirection.Horizontal ) {
                _height = ( aHeight > height ) ? aHeight : height;
            }
            else if ( direction == LayoutDirection.Vertical ) {
                _height += aHeight;
            }
        }

        public void AdjustWidth( float aWidth ) {
            if ( direction == LayoutDirection.Vertical ) {
                _width = ( aWidth > width ) ? aWidth : width;
            }
            else if ( direction == LayoutDirection.Horizontal ) {
                _width += aWidth;
            }
        }
    }

    private enum LayoutDirection {
        Horizontal,
        Vertical
    }

    protected GUIStyle CreateLabelStyle( TextAnchor aAnchor, int aFontSize = DEFAULT_FONT_SIZE ) {
        var lStyle = new GUIStyle( GUI.skin.label );
        lStyle.alignment = aAnchor;
        lStyle.fontSize = aFontSize;
        lStyle.fixedHeight = aFontSize;
        return lStyle;
    }


    public const int DEFAULT_POSITION_HEIGHT = 16;
    public const int DEFAULT_LINE_SPACE_HEIGHT = 4;
    public const int DEFAULT_SPACE_HEIGHT = 16;
    public const int DEFAULT_FONT_SIZE = 12;
    public const int FOLDOUT_END_SPACE = 3;
    public const TextAnchor DEFAULT_TEXT_ALIGNMENT = TextAnchor.MiddleLeft;

    protected float propertyHeight = 0;
    private float _fieldWidth = 0;
    private float _fieldHeight = 0;
    private float _lastFieldHeight = 0;
    protected Rect startPosition;
    protected Rect currentPosition;
    public SerializedProperty _property;

    private List<LayoutDirectionRect> LayoutDirectionRectStack = new List<LayoutDirectionRect>();
    private LayoutDirectionRect currentDirectionRect => LayoutDirectionRectStack[LayoutDirectionRectStack.Count - 1];

    public SerializedProperty Property => _property;
    public float fieldWidth => ( _fieldWidth > 0 ) ? _fieldWidth : startPosition.width;
    public float fieldHeight => ( _fieldHeight > 0 ) ? _fieldHeight : startPosition.height;
    public float maxWidth => startPosition.width;

    #region Overrides
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        return EditorGUI.GetPropertyHeight( property, true ) + propertyHeight - _lastFieldHeight;
    }
    #endregion

    #region Configuration
    protected float GetHeight( SerializedProperty aProperty, string aPropertyName ) {
        return EditorGUI.GetPropertyHeight( aProperty.FindPropertyRelative( aPropertyName ), true );
    }

    /// <summary>
    /// Sets the width of fields. A value of less than 1 will result in the default field width.
    /// </summary>
    protected void SetFieldWidth( float aWidth ) {
        aWidth = Mathf.Clamp( aWidth, 0, Screen.width - 25 );
        _fieldWidth = aWidth;
        currentPosition.width = (aWidth > 0) ? aWidth : maxWidth;
    }

    /// <summary>
    /// 
    /// </summary>
    protected void SetFieldHeight( float aHeight ) {
        if( aHeight < 1 ) {
            aHeight = DEFAULT_POSITION_HEIGHT;
        }

        _fieldHeight = aHeight;
        currentPosition.height = aHeight;
    }
    #endregion

    #region Drawing
    /// <summary>
    /// Call this at the beginning of the custom property.
    /// </summary>
    public GUIContent BeginProperty( ref Rect aPosition, SerializedProperty aProperty, GUIContent aLabel ) {
        _property = aProperty;
        propertyHeight = 0;

        //For whatever reason, ref is needed to do this outside of OnGUI
        aPosition.height = DEFAULT_POSITION_HEIGHT;

        currentPosition = aPosition;
        startPosition = aPosition;
        BeginVertical();
        return EditorGUI.BeginProperty( aPosition, aLabel, aProperty );
    }

    /// <summary>
    /// Call this at the end of the custom property.
    /// </summary>
    public void EndProperty() {
        float lHeight = currentDirectionRect.height;
        InternalEndVertical( 0 );
        propertyHeight += lHeight;
        EditorGUI.EndProperty();
        EditorUtility.SetDirty( _property.serializedObject.targetObject );
    }

    private Rect NewLine( Rect aPosition, SerializedProperty aProperty, string aPropertyName ) {
        return new Rect( aPosition.x, aPosition.y + fieldHeight, aPosition.width, aPosition.height );
    }

    /// <summary>
    /// Draws a property field with the supplied name using the supplied SerializedProperty.
    /// </summary>
    protected void DrawField( SerializedProperty aProperty, string aPropertyName ) {
        EditorGUI.PropertyField( currentPosition, aProperty.FindPropertyRelative( aPropertyName ), true );

        if ( currentDirectionRect.direction == LayoutDirection.Vertical ) {
            //currentPosition = NewLine( currentPosition, aProperty, aPropertyName );
            currentPosition = new Rect( currentPosition.x, currentPosition.y + fieldHeight + DEFAULT_LINE_SPACE_HEIGHT, currentPosition.width, currentPosition.height );
            currentDirectionRect.AdjustHeight( fieldHeight + DEFAULT_LINE_SPACE_HEIGHT );
        }
        else if ( currentDirectionRect.direction == LayoutDirection.Horizontal ) {
            currentPosition = new Rect( currentPosition.x + fieldWidth, currentPosition.y, currentPosition.width, currentPosition.height );
            currentDirectionRect.AdjustHeight( fieldHeight );
        }
        currentDirectionRect.AdjustWidth( fieldWidth );
    }

    /// <summary>
    /// Draws a property field with the supplied property name using the base SerializedProperty.
    /// </summary>
    protected void DrawField( string aPropertyName ) {
        DrawField( _property, aPropertyName );
    }

    /// <summary>
    /// Call this whenever drawing a property field outside of DrawField() and DrawLabel() to
    /// calculate it's size into the drawer height and layout group dimensions.
    /// </summary>
    protected void AddSize( int aWidth, int aHeight ) {
        currentDirectionRect.AdjustWidth( aWidth );
        currentDirectionRect.AdjustHeight( aHeight );
    }

    /// <summary>
    /// Draws a label using the provided string.
    /// </summary>
    protected void DrawLabel( string aString ) {
        DrawLabel( aString, DEFAULT_TEXT_ALIGNMENT, DEFAULT_FONT_SIZE );
    }

    /// <summary>
    /// Draws a label using the provided string and text alignment.
    /// </summary>
    protected void DrawLabel( string aString, TextAnchor aAlignment ) {
        DrawLabel( aString, aAlignment, DEFAULT_FONT_SIZE );
    }

    /// <summary>
    /// Draws a label using the provided string, text alignment, and font size.
    /// </summary>
    protected void DrawLabel( string aString, TextAnchor aAlignment, int aFontSize ) {
        if( aAlignment == DEFAULT_TEXT_ALIGNMENT && aFontSize == DEFAULT_FONT_SIZE ) {
            EditorGUI.LabelField( currentPosition, aString );
        }
        else {
            EditorGUI.LabelField( currentPosition, aString, CreateLabelStyle( aAlignment, aFontSize ) );
        }

        //Override the current rect height to fit label size if needed
        float lCachedHeight = currentPosition.height;
        float lLabelHeight = ( fieldHeight > aFontSize ) ? fieldHeight : aFontSize;
        if ( fieldHeight > aFontSize ) {
            currentPosition = new Rect( currentPosition.x, currentPosition.y, currentPosition.width, aFontSize );
        }

        if( currentDirectionRect.direction == LayoutDirection.Vertical ) {
            currentPosition = new Rect( currentPosition.x, currentPosition.y + lLabelHeight + DEFAULT_LINE_SPACE_HEIGHT, currentPosition.width, currentPosition.height );
            currentDirectionRect.AdjustHeight( lLabelHeight + DEFAULT_LINE_SPACE_HEIGHT );
        }
        else if( currentDirectionRect.direction == LayoutDirection.Horizontal ) {
            currentPosition = new Rect( currentPosition.x + fieldWidth, currentPosition.y, currentPosition.width, currentPosition.height );
            currentDirectionRect.AdjustHeight( lLabelHeight );
        }
        currentDirectionRect.AdjustWidth( fieldWidth );

        //Set the current rect height to the cached value
        if ( fieldHeight > aFontSize ) {
            currentPosition = new Rect( currentPosition.x, currentPosition.y, currentPosition.width, lCachedHeight );
        }

        _lastFieldHeight = lLabelHeight;
    }

    /// <summary>
    /// Draws an empty space using the defualt space height or a specified height.
    /// </summary>
    protected void DrawSpace( int aHeight = DEFAULT_SPACE_HEIGHT ) {
        DrawSpace( true, DEFAULT_SPACE_HEIGHT );
    }

    /// <summary>
    /// Used internally for layout groups.
    /// </summary>
    private void DrawSpace( bool aCacheHeight, int aHeight = DEFAULT_SPACE_HEIGHT ) {
        currentPosition = new Rect( currentPosition.x, currentPosition.y + aHeight, currentPosition.width, currentPosition.height );
        currentDirectionRect.AdjustHeight( aHeight );
        if ( aCacheHeight ) {
            _lastFieldHeight = aHeight;
        }
    }

    /// <summary>
    /// Draws a foldout and it's content when opened.
    /// </summary>
    protected void DrawFoldout( bool aFoldout, string aLabel, System.Action aDrawAction, out bool aIsOpen, bool aToggleOnLabelClick = true ) {
        aIsOpen = EditorGUI.Foldout( currentPosition, aFoldout, aLabel, aToggleOnLabelClick, EditorStyles.foldoutHeader);
        DrawSpace();
        if ( aFoldout ) {
            DrawSpace( false, FOLDOUT_END_SPACE );
            EditorGUI.indentLevel++;
            aDrawAction?.Invoke();
            EditorGUI.indentLevel--;
            DrawSpace( false, FOLDOUT_END_SPACE );
        }
    }

    #endregion

    #region Layout
    protected void BeginVertical() {
        LayoutDirectionRectStack.Add( new LayoutDirectionRect( currentPosition.x, currentPosition.y, LayoutDirection.Vertical ) );
    }

    protected void EndVertical() {
        InternalEndVertical( 1 );
    }

    //The stop index is intended to prevent a derived class from ending the internal vertical layout group wrapper.
    private void InternalEndVertical( int aStopIndex ) {
        for ( int i = LayoutDirectionRectStack.Count - 1; i >= aStopIndex; i-- ) {
            if( LayoutDirectionRectStack[i].direction == LayoutDirection.Vertical ) {

                if ( i > 0 && currentDirectionRect == LayoutDirectionRectStack[i] ) {
                    //Update current position to previous layout rect
                    if ( LayoutDirectionRectStack[i - 1].direction == LayoutDirection.Vertical ) {
                        currentPosition = new Rect( LayoutDirectionRectStack[i - 1].x, currentPosition.y + currentDirectionRect.height, currentPosition.width, currentPosition.height );
                    }
                    else if ( LayoutDirectionRectStack[i - 1].direction == LayoutDirection.Horizontal ) {
                        currentPosition = new Rect( currentPosition.x + currentDirectionRect.width, LayoutDirectionRectStack[i - 1].y, currentPosition.width, currentPosition.height );
                    }
                }

                float lWidth = currentDirectionRect.width;
                float lHeight = currentDirectionRect.height;

                LayoutDirectionRectStack.Remove( LayoutDirectionRectStack[i] );

                if( i != 0 ) {
                    currentDirectionRect?.AdjustWidth( lWidth );
                    currentDirectionRect?.AdjustHeight( lHeight );
                }

                return;
            }
        }

        Debug.LogError( $"[{nameof( PropertyDrawerBase )}] - End of stack occured before vertical layout could end." );
    }

    protected void BeginHorizontal() {
        LayoutDirectionRectStack.Add( new LayoutDirectionRect( currentPosition.x, currentPosition.y, LayoutDirection.Horizontal ) );
    }

    protected void EndHorizontal() {
        for ( int i = LayoutDirectionRectStack.Count - 1; i >= 0; i-- ) {
            if ( LayoutDirectionRectStack[i].direction == LayoutDirection.Horizontal ) {
               
                if( i > 0 && currentDirectionRect == LayoutDirectionRectStack[i] ) {
                    //Update current position to previous layout rect
                    if( LayoutDirectionRectStack[i - 1].direction == LayoutDirection.Vertical ) {
                        currentPosition = new Rect( LayoutDirectionRectStack[i - 1].x, currentPosition.y + currentDirectionRect.height, currentPosition.width, currentPosition.height );
                    } 
                    else if( LayoutDirectionRectStack[i - 1].direction == LayoutDirection.Horizontal ) {
                        currentPosition = new Rect( currentPosition.x + currentDirectionRect.width, LayoutDirectionRectStack[i - 1].y, currentPosition.width, currentPosition.height );
                    } 
                }

                float lWidth = currentDirectionRect.width;
                float lHeight = currentDirectionRect.height;

                LayoutDirectionRectStack.Remove( LayoutDirectionRectStack[i] );

                if ( i != 0 ) {
                    currentDirectionRect?.AdjustWidth( lWidth );
                    currentDirectionRect?.AdjustHeight( lHeight );
                }

                return;
            }
        }

        Debug.LogError( $"[{nameof( PropertyDrawerBase )}] - End of stack occured before horizontal layout could end." );
    }
    #endregion
}
