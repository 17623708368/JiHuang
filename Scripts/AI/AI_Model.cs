using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 怪物的模型层
/// </summary>
public class AI_Model : MonoBehaviour
{
   private AIBase parentContorller;

   private void Start()
   {
      parentContorller = GetComponentInParent<AIBase>();
   }

   public void AnimationEvent(string name)
   {
      parentContorller.AnimationAcitonEvent(name);
   }
}
