using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MapObjectBase,IBuilding
{
   [SerializeField]private new Collider  colider;
   private List<Material> materialsList=null;

   #region 预览模式
   public GameObject GameObject { get=>gameObject; }

   public  Collider  Collider { get=>colider;   }
   public List<Material> MaterialsList { get; set; }

   #endregion

   public virtual void OnSelect()
   {
      
   }
   public virtual bool OnSlotEndDragSelect(int itemID)
   {
      return false;
   }
   public abstract void Onpreview();
}
