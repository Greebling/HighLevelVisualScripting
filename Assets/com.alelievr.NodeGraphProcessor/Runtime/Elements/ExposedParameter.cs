using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	[Serializable]
	public class ExposedParameter : ISerializationCallbackReceiver
	{
        [Serializable]
        public class Settings
        {
            public bool isHidden = false;
            public bool expanded = false;

            [SerializeField]
            internal string guid = null;

            public override bool Equals(object obj)
            {
                if (obj is Settings s && s != null)
                    return Equals(s);
                else
                    return false;
            }

            public virtual bool Equals(Settings param)
                => isHidden == param.isHidden && expanded == param.expanded;

            public override int GetHashCode() => base.GetHashCode();
        }

		public string				guid; // unique id to keep track of the parameter
		public string				name;
		[Obsolete("Use GetValueType()")]
		public string				type;
		[Obsolete("Use value instead")]
		public SerializableObject	serializedValue;
		public bool					input = true;
        [SerializeReference]
		public Settings             settings;
		public string shortType => GetValueType()?.Name;

        public void Initialize(string name, object value)
        {
			guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter
            settings = CreateSettings();
            settings.guid = guid;
			this.name = name;
			this.value = value;
        }

        public static ExposedParameter CopyParameter(ExposedParameter parameter)
        {
	        var copy = Activator.CreateInstance(parameter.GetType()) as ExposedParameter;
	        copy.name = parameter.name;
	        copy.guid = parameter.guid;
	        copy.value = parameter.value;
	        copy.settings = parameter.settings;
	        return copy;
        }

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// SerializeReference migration step:
#pragma warning disable CS0618
			if (serializedValue?.value != null) // old serialization system can't serialize null values
			{
				value = serializedValue.value;
				Debug.Log("Migrated: " + serializedValue.value + " | " + serializedValue.serializedName);
				serializedValue.value = null;
			}
#pragma warning restore CS0618
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {}

        protected virtual Settings CreateSettings() => new Settings();

        public virtual object value { get; set; }
        public virtual Type GetValueType() => value == null ? typeof(object) : value.GetType();

        static Dictionary<Type, Type> exposedParameterTypeCache = new Dictionary<Type, Type>();
        internal ExposedParameter Migrate()
        {
            if (exposedParameterTypeCache.Count == 0)
            {
                foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
                {
                    if (type.IsSubclassOf(typeof(ExposedParameter)) && !type.IsAbstract)
                    {
                        var paramType = Activator.CreateInstance(type) as ExposedParameter;
                        exposedParameterTypeCache[paramType.GetValueType()] = type;
                    }
                }
            }
#pragma warning disable CS0618 // Use of obsolete fields
            var oldType = Type.GetType(type);
#pragma warning restore CS0618
            if (oldType == null || !exposedParameterTypeCache.TryGetValue(oldType, out var newParamType))
                return null;
            
            var newParam = Activator.CreateInstance(newParamType) as ExposedParameter;

            newParam.guid = guid;
            newParam.name = name;
            newParam.input = input;
            newParam.settings = newParam.CreateSettings();
            newParam.settings.guid = guid;

            return newParam;
     
        }

        public static bool operator ==(ExposedParameter param1, ExposedParameter param2)
        {
            if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))
                return true;
            if (ReferenceEquals(param1, param2))
                return true;
            if (ReferenceEquals(param1, null))
                return false;
            if (ReferenceEquals(param2, null))
                return false;

            return param1.Equals(param2);
        }

        public static bool operator !=(ExposedParameter param1, ExposedParameter param2) => !(param1 == param2);

        public bool Equals(ExposedParameter parameter) => guid == parameter.guid;

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            else
                return Equals((ExposedParameter)obj);
        }

        public override int GetHashCode() => guid.GetHashCode();

        public ExposedParameter Clone()
        {
            var clonedParam = Activator.CreateInstance(GetType()) as ExposedParameter;

            clonedParam.guid = guid;
            clonedParam.name = name;
            clonedParam.input = input;
            clonedParam.settings = settings;
            clonedParam.value = value;

            return clonedParam;
        }

        public virtual VisualElement GetPropertyDrawer()
        {
	        var objField = new ObjectField();
	        objField.objectType = GetValueType();
	        objField.label = "";
	        objField.allowSceneObjects = false;
	        objField.style.flexGrow = new StyleFloat(1.0f);
	        objField.style.width =
		        0; //so it does not have such a long minimum width and is aligned with other fields
	        objField.RegisterValueChangedCallback(evt => value = evt.newValue);

	        if (value != null)
		        objField.value = (UnityEngine.Object)value;
	        return objField;
        }
	}

    // Due to polymorphic constraints with [SerializeReference] we need to explicitly create a class for
    // every parameter type available in the graph (i.e. templating doesn't work)
    [System.Serializable]
    public class ColorParameter : ExposedParameter
    {
        public enum ColorMode
        {
            Default,
            HDR
        }

        [Serializable]
        public class ColorSettings : Settings
        {
            public ColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((ColorSettings)param).mode;
        }

        [SerializeField] Color val;

        public override object value { get => val; set => val = (Color)value; }
        protected override Settings CreateSettings() => new ColorSettings();

        public override VisualElement GetPropertyDrawer()
        {
	        var colorField = new ColorField();
	        colorField.value = val;
	        colorField.RegisterValueChangedCallback(evt => value = evt.newValue);
	        colorField.style.width = 0;
	        colorField.style.flexGrow = new StyleFloat(1.0f);

	        colorField.value = val;
	        return colorField;
        }
    }

    [System.Serializable]
    public class FloatParameter : ExposedParameter
    {
        public enum FloatMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class FloatSettings : Settings
        {
            public FloatMode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((FloatSettings)param).mode && min == ((FloatSettings)param).min && max == ((FloatSettings)param).max;
        }

        [SerializeField] float val;

        public override object value { get => val; set => val = (float)value; }
        protected override Settings CreateSettings() => new FloatSettings();

        public override VisualElement GetPropertyDrawer()
        {
	        var floatField = new FloatField();
	        floatField.value = val;
	        floatField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        floatField.style.width = 0;
	        floatField.style.flexGrow = new StyleFloat(1.0f);
	        return floatField;
        }
    }

    [System.Serializable]
    public class Vector2Parameter : ExposedParameter
    {
        public enum Vector2Mode
        {
            Default,
            MinMaxSlider,
        }

        [Serializable]
        public class Vector2Settings : Settings
        {
            public Vector2Mode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((Vector2Settings)param).mode && min == ((Vector2Settings)param).min && max == ((Vector2Settings)param).max;
        }

        [SerializeField] Vector2 val;

        public override object value { get => val; set => val = (Vector2)value; }
        protected override Settings CreateSettings() => new Vector2Settings();

        public override VisualElement GetPropertyDrawer()
        {
	        var vector2Field = new Vector2Field();
	        vector2Field.RegisterValueChangedCallback(evt => val = evt.newValue);
	        vector2Field.style.width = 0;
	        vector2Field.style.flexGrow = new StyleFloat(1.0f);

	        vector2Field.value = val;
	        return vector2Field;
        }
    }

    [System.Serializable]
    public class Vector3Parameter : ExposedParameter
    {
        [SerializeField] Vector3 val;

        public override object value { get => val; set => val = (Vector3)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var vector3Field = new Vector3Field();
	        vector3Field.RegisterValueChangedCallback(evt => val = evt.newValue);
	        vector3Field.style.width = 0;
	        vector3Field.style.flexGrow = new StyleFloat(1.0f);

	        vector3Field.value = val;
	        return vector3Field;
        }
    }

    [System.Serializable]
    public class Vector4Parameter : ExposedParameter
    {
        [SerializeField] Vector4 val;

        public override object value { get => val; set => val = (Vector4)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var vector4Field = new Vector4Field();
	        vector4Field.RegisterValueChangedCallback(evt => val = evt.newValue);
	        vector4Field.style.width = 0;
	        vector4Field.style.flexGrow = new StyleFloat(1.0f);

	        vector4Field.value = val;
	        return vector4Field;
        }
    }

    [System.Serializable]
    public class IntParameter : ExposedParameter
    {
        public enum IntMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class IntSettings : Settings
        {
            public IntMode mode;
            public int min = 0;
            public int max = 10;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((IntSettings)param).mode && min == ((IntSettings)param).min && max == ((IntSettings)param).max;
        }

        [SerializeField] int val;

        public override object value { get => val; set => val = (int)value; }
        protected override Settings CreateSettings() => new IntSettings();

        public override VisualElement GetPropertyDrawer()
        {
	        var intField = new IntegerField();
	        intField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        intField.style.width = 0;
	        intField.style.flexGrow = new StyleFloat(1.0f);

	        intField.value = val;
	        return intField;
        }
    }

    [System.Serializable]
    public class Vector2IntParameter : ExposedParameter
    {
        [SerializeField] Vector2Int val;

        public override object value { get => val; set => val = (Vector2Int)value; }

        public override VisualElement GetPropertyDrawer()
        {
	        var vector2Field = new Vector2IntField();
	        vector2Field.RegisterValueChangedCallback(evt => val = evt.newValue);
	        vector2Field.style.width = 0;
	        vector2Field.style.flexGrow = new StyleFloat(1.0f);

	        vector2Field.value = val;
	        return vector2Field;
        }
    }

    [System.Serializable]
    public class Vector3IntParameter : ExposedParameter
    {
        [SerializeField] Vector3Int val;

        public override object value { get => val; set => val = (Vector3Int)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var vector3Field = new Vector3IntField();
	        vector3Field.RegisterValueChangedCallback(evt => val = evt.newValue);
	        vector3Field.style.width = 0;
	        vector3Field.style.flexGrow = new StyleFloat(1.0f);

	        vector3Field.value = val;
	        return vector3Field;
        }
    }

    [System.Serializable]
    public class DoubleParameter : ExposedParameter
    {
        [SerializeField] Double val;

        public override object value { get => val; set => val = (Double)value; }

        public override VisualElement GetPropertyDrawer()
        {
	        var doubleField = new DoubleField();
	        doubleField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        doubleField.style.width = 0;
	        doubleField.style.flexGrow = new StyleFloat(1.0f);

	        doubleField.value = val;
	        return doubleField;
        }
    }

    [System.Serializable]
    public class LongParameter : ExposedParameter
    {
        [SerializeField] long val;

        public override object value { get => val; set => val = (long)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var longField = new LongField();
	        longField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        longField.style.width = 0;
	        longField.style.flexGrow = new StyleFloat(1.0f);

	        longField.value = val;
	        return longField;
        }
    }

    [System.Serializable]
    public class StringParameter : ExposedParameter
    {
        [SerializeField] string val;

        public override object value { get => val; set => val = (string)value; }
        public override Type GetValueType() => typeof(String);

        public override VisualElement GetPropertyDrawer()
        {
	        var stringField = new TextField();
	        stringField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        stringField.style.width = 0;
	        stringField.style.flexGrow = new StyleFloat(1.0f);

	        if (val != null)
		        stringField.value = val;
	        return stringField;
        }
    }

    [System.Serializable]
    public class RectParameter : ExposedParameter
    {
        [SerializeField] Rect val;

        public override object value { get => val; set => val = (Rect)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var rectField = new RectField();
	        rectField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        rectField.style.width = 0;
	        rectField.style.flexGrow = new StyleFloat(1.0f);

	        if (val != null)
		        rectField.value = val;
	        return rectField;
        }
    }

    [System.Serializable]
    public class RectIntParameter : ExposedParameter
    {
        [SerializeField] RectInt val;

        public override object value { get => val; set => val = (RectInt)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var rectField = new RectIntField();
	        rectField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        rectField.style.width = 0;
	        rectField.style.flexGrow = new StyleFloat(1.0f);

	        rectField.value = val;
	        return rectField;
        }
    }

    [System.Serializable]
    public class BoundsParameter : ExposedParameter
    {
        [SerializeField] Bounds val;

        public override object value { get => val; set => val = (Bounds)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var boundsField = new BoundsField();
	        boundsField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        boundsField.style.width = 0;
	        boundsField.style.flexGrow = new StyleFloat(1.0f);

	        if (val != null)
		        boundsField.value = val;
	        return boundsField;
        }
    }

    [System.Serializable]
    public class BoundsIntParameter : ExposedParameter
    {
        [SerializeField] BoundsInt val;

        public override object value { get => val; set => val = (BoundsInt)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var boundsField = new BoundsIntField();
	        boundsField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        boundsField.style.width = 0;
	        boundsField.style.flexGrow = new StyleFloat(1.0f);
	        
	        if (val != null) 
		        boundsField.value = val;
	        return boundsField;
        }
    }

    [System.Serializable]
    public class AnimationCurveParameter : ExposedParameter
    {
        [SerializeField] AnimationCurve val;

        public override object value { get => val; set => val = (AnimationCurve)value; }
        public override Type GetValueType() => typeof(AnimationCurve);
    }

    [System.Serializable]
    public class GradientParameter : ExposedParameter
    {
        public enum GradientColorMode
        {
            Default,
            HDR,
        }

        [Serializable]
        public class GradientSettings : Settings
        {
            public GradientColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((GradientSettings)param).mode;
        }

        [SerializeField] Gradient val;
        [SerializeField, GradientUsage(true)] Gradient hdrVal;

        public override object value { get => val; set => val = (Gradient)value; }
        public override Type GetValueType() => typeof(Gradient);
        protected override Settings CreateSettings() => new GradientSettings();
        
        public override VisualElement GetPropertyDrawer()
        {
	        var gradientField = new GradientField();
	        gradientField.RegisterValueChangedCallback(evt => val = evt.newValue);
	        gradientField.style.width = 0;
	        gradientField.style.flexGrow = new StyleFloat(1.0f);
	        
	        if (val != null) 
		        gradientField.value = val;
	        return gradientField;
        }
    }

    [System.Serializable]
    public class GameObjectParameter : ExposedParameter
    {
        [SerializeField] GameObject val;

        public override object value { get => val; set => val = (GameObject)value; }
        public override Type GetValueType() => typeof(GameObject);
    }

    [System.Serializable]
    public class BoolParameter : ExposedParameter
    {
        [SerializeField] bool val;

        public override object value { get => val; set => val = (bool)value; }
        
        public override VisualElement GetPropertyDrawer()
        {
	        var boolField = new Button();
	        boolField.text = val ? "Yes" : "No ";
	        boolField.clicked += () =>
	        {
		        val = !val;
		        boolField.text = val ? "Yes" : "No ";
	        };
	        // not sure why, but this is needed aligns it with other entries:
	        boolField.style.marginLeft = new Length(12, LengthUnit.Pixel);
	        boolField.style.width = 0;
	        boolField.style.flexGrow = new StyleFloat(1.0f);
	        return boolField;
        }
    }

    [System.Serializable]
    public class Texture2DParameter : ExposedParameter
    {
        [SerializeField] Texture2D val;

        public override object value { get => val; set => val = (Texture2D)value; }
        public override Type GetValueType() => typeof(Texture2D);
    }

    [System.Serializable]
    public class RenderTextureParameter : ExposedParameter
    {
        [SerializeField] RenderTexture val;

        public override object value { get => val; set => val = (RenderTexture)value; }
        public override Type GetValueType() => typeof(RenderTexture);
    }

    [System.Serializable]
    public class MeshParameter : ExposedParameter
    {
        [SerializeField] Mesh val;

        public override object value { get => val; set => val = (Mesh)value; }
        public override Type GetValueType() => typeof(Mesh);
    }

    [System.Serializable]
    public class MaterialParameter : ExposedParameter
    {
        [SerializeField] Material val;

        public override object value { get => val; set => val = (Material)value; }
        public override Type GetValueType() => typeof(Material);
    }
    
    
    [System.Serializable]
    public class GenericParameter<T> : ExposedParameter
    {
	    [SerializeField] T val;

	    public override object value { get => val; set => val = (T)value; }
	    public override Type GetValueType() => typeof(T);
    }
}