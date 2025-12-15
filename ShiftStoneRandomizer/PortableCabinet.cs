using Il2CppTMPro;
using MelonLoader;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using RumbleModdingAPI;
using System.Collections.Generic;
using System.Linq;
using System;
using Il2CppRUMBLE.Managers;
using UnityEngine.Bindings;

namespace ShiftStoneRandomizer
{
	public class PortableCabinet
    {
        private static GameObject qssReplacementBase;
        private static GameObject ShiftStoneBoxSource;
        /// <summary>
        /// Static constructor: Only runs once. Make sure no reference to portable cabinet is made before first load
        /// </summary>
        static PortableCabinet()
        {
            qssReplacementBase = GameObject.Instantiate( /*Swapper button in API*/);
            ShiftStoneBoxSource = GameObject.Instantiate(Calls.GameObjects.Gym.LOGIC.Heinhouserproducts.ShiftstoneCabinet.Cabinet.ShiftstoneBox___________.GetGameObject());
        }


        private GameObject Cubbies {get; private set;}
        public GameObject StoneCase {get; private set; }
        private GameObject NewQssButton {get; set; }
        
        public PortableCabinet(GameObject swapper)
        {   
            double mulCol = 0.1; double mulRow = 0.12;

            StoneCase = new GameObject("Portable Stone Case");

            for (int i = 0; i < 12; i++;)
            {
                ShiftStonePrefs currentStonePref = i < 8 ? (ShiftStonePrefs) i : (ShiftStonePrefs) (i - 12);

                GameObject box = GameObject.Instantiate(ShiftStoneBoxSource);
                box.name = $"{currentStonePref.ToString()}_Case";
                box.transform.localPosition = new Vector3((float)(mulCol * (i % 4) * -1), (float)(mulRow * (Math.Floor(i / 4) * -1)), 0.07f);
                box.transform.rotation = Quaternion.Euler(0, 90, 0);
                box.transform.GetChild(0).localPosition = new Vector3(-0.03f, -0.04f, -0f);
                box.transform.SetParent(StoneCase.transform, false);
                box.SetActive(true);

                if(i < 8)
                {
                    StoneItem.AllStones[i].AddIcon(ShiftStoneRandomizer.CreateBlackListIcons(box));
                }

                GameObject boxDisplay = StoneItem.GetDisplayObject(currentStonePref);
                //Standard shift stones should be rotated on their broad face to show off their outline. But charge's outline is clearer from the side				
                if (currentStoneItem >= 0 && currentStoneItem != ShiftStonePrefs.Charge)
				{
					boxDisplay.transform.rotation = Quaternion.Euler(0, 0, 90);
				}
                //The current placeholder icons for control stones are best displayed rotated this way
				else if (currentStoneItem < 0)
				{
					boxDisplay.transform.rotation = Quaternion.Euler(0, 90, 0);
				}

                
            }



            NewQssButton = GameObject.Instantiate(qssReplacementBase);




        }


    }
}