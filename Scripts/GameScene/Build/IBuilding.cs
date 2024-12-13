    using System.Collections.Generic;
    using UnityEngine;

    public interface IBuilding
    {
        public  Collider  Collider { get;   }
        public List<Material> MaterialsList { get; set; }
        public static Color red = new Color(1, 0, 0, 0.5f);
        public static Color green = new Color(0, 1, 0, 0.5f);
        public GameObject GameObject { get; }
        public virtual void InitOnPreview()
        {
            Collider.enabled=false;
            MaterialsList = new List<Material>(10);
            MeshRenderer[]   MeshRenders = GameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < MeshRenders.Length; i++)
            {
                MaterialsList.AddRange(MeshRenders[i].materials);
            }

            for (int i = 0; i < MaterialsList.Count; i++)
            {
                MaterialsList[i].color=red;
                ProjectTool.SetMaterialRenderingMode(MaterialsList[i],ProjectTool.RenderingMode.Fade);
            }

            Onpreview();
        }

        public void Onpreview()     { }
   
        public void SetMaterialsColor(bool isRed)
        {
            for (int i = 0; i < MaterialsList.Count; i++)
            {
                MaterialsList[i].color=isRed?red: green;
            }
        }

    }
