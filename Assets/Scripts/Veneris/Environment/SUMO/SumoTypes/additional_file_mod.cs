/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// Este código fuente fue generado automáticamente por xsd, Versión=4.6.1055.0.
// 
namespace Veneris {
    using System.Xml.Serialization;
    
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("additional", Namespace="", IsNullable=false)]
    public partial class additionalType {
        
        private object[] itemsField;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("include", typeof(additionalTypeInclude))]
        [System.Xml.Serialization.XmlElementAttribute("location", typeof(locationType))]
        [System.Xml.Serialization.XmlElementAttribute("poi", typeof(poiType))]
        [System.Xml.Serialization.XmlElementAttribute("poly", typeof(polygonType))]
        public object[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class additionalTypeInclude {
        
        private string hrefField;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string href {
            get {
                return this.hrefField;
            }
            set {
                this.hrefField = value;
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class poiType {
        
        private paramType[] paramField;
        
        private string idField;
        
        private string typeField;
        
        private string colorField;
        
        private float layerField;
        
        private bool layerFieldSpecified;
        
        private float xField;
        
        private bool xFieldSpecified;
        
        private float yField;
        
        private bool yFieldSpecified;
        
        private float lonField;
        
        private bool lonFieldSpecified;
        
        private float latField;
        
        private bool latFieldSpecified;
        
        private string laneField;
        
        private float posField;
        
        private bool posFieldSpecified;
        
        private float angleField;
        
        private bool angleFieldSpecified;
        
        private string imgFileField;
        
        private float widthField;
        
        private bool widthFieldSpecified;
        
        private float heightField;
        
        private bool heightFieldSpecified;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("param")]
        public paramType[] param {
            get {
                return this.paramField;
            }
            set {
                this.paramField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color {
            get {
                return this.colorField;
            }
            set {
                this.colorField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float layer {
            get {
                return this.layerField;
            }
            set {
                this.layerField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool layerSpecified {
            get {
                return this.layerFieldSpecified;
            }
            set {
                this.layerFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float x {
            get {
                return this.xField;
            }
            set {
                this.xField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool xSpecified {
            get {
                return this.xFieldSpecified;
            }
            set {
                this.xFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float y {
            get {
                return this.yField;
            }
            set {
                this.yField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ySpecified {
            get {
                return this.yFieldSpecified;
            }
            set {
                this.yFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float lon {
            get {
                return this.lonField;
            }
            set {
                this.lonField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lonSpecified {
            get {
                return this.lonFieldSpecified;
            }
            set {
                this.lonFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float lat {
            get {
                return this.latField;
            }
            set {
                this.latField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool latSpecified {
            get {
                return this.latFieldSpecified;
            }
            set {
                this.latFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string lane {
            get {
                return this.laneField;
            }
            set {
                this.laneField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float pos {
            get {
                return this.posField;
            }
            set {
                this.posField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool posSpecified {
            get {
                return this.posFieldSpecified;
            }
            set {
                this.posFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float angle {
            get {
                return this.angleField;
            }
            set {
                this.angleField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool angleSpecified {
            get {
                return this.angleFieldSpecified;
            }
            set {
                this.angleFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string imgFile {
            get {
                return this.imgFileField;
            }
            set {
                this.imgFileField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool widthSpecified {
            get {
                return this.widthFieldSpecified;
            }
            set {
                this.widthFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool heightSpecified {
            get {
                return this.heightFieldSpecified;
            }
            set {
                this.heightFieldSpecified = value;
            }
        }
    }
    
   
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class polygonType {
        
        private paramType[] paramField;
        
        private string idField;
        
        private string typeField;
        
        private string colorField;
        
        private boolType fillField;
        
        private bool fillFieldSpecified;
        
        private boolType geoField;
        
        private bool geoFieldSpecified;
        
        private float layerField;
        
        private bool layerFieldSpecified;
        
        private string shapeField;
        
        private float angleField;
        
        private bool angleFieldSpecified;
        
        private string imgFileField;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("param")]
        public paramType[] param {
            get {
                return this.paramField;
            }
            set {
                this.paramField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color {
            get {
                return this.colorField;
            }
            set {
                this.colorField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public boolType fill {
            get {
                return this.fillField;
            }
            set {
                this.fillField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fillSpecified {
            get {
                return this.fillFieldSpecified;
            }
            set {
                this.fillFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public boolType geo {
            get {
                return this.geoField;
            }
            set {
                this.geoField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool geoSpecified {
            get {
                return this.geoFieldSpecified;
            }
            set {
                this.geoFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float layer {
            get {
                return this.layerField;
            }
            set {
                this.layerField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool layerSpecified {
            get {
                return this.layerFieldSpecified;
            }
            set {
                this.layerFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string shape {
            get {
                return this.shapeField;
            }
            set {
                this.shapeField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float angle {
            get {
                return this.angleField;
            }
            set {
                this.angleField = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool angleSpecified {
            get {
                return this.angleFieldSpecified;
            }
            set {
                this.angleFieldSpecified = value;
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string imgFile {
            get {
                return this.imgFileField;
            }
            set {
                this.imgFileField = value;
            }
        }
    }
    
    
  
}