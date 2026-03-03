# **MANAGER SYSTEM**



### **CONSTRUCTION:**

* **Add Game prefab to the scene from Jaakko/Prefabs // in final move this obj into a bootstrap scene**
* **There must be only one obj with Game.cs but Game.cs handles this internally**
* **Game.cs will create GameManager and all other sub managers such as InputManager, SaveManager, InventoryManager ect..**



### **USAGE:**

* Dependencies can be retrieved from Services.Get<>(); ie. Services.Get<InputManager>(); THIS IS NOT RECOMMENDED AS THIS MAY PRODUCE RACE CONDITIONS. See proper usage below:



* GameObjects that need dependencies (Input, Inventory) or need save states should have or inherit from Actor.cs
* Dependencies should be gathered and cached in Actor.cs Init()
* Actor.cs will automatically call Load / Save on all scripts on the same gameObject that have IActorComponent



### **ACTOR:**

* Player Actor should be tagged as Player so it can be referenced by other Actors
* Actors handle Saving / Loading Data as well as initializing when all other systems are ready
* Actors will add HealthComponent and InventoryComponent to themselves, they should not be added in the inspector // this can be changed if necessary 
* Other future components that Actor needs should be added in Actor.cs Init();



### **ACTOR COMPONENT:**

* Scripts on the same gameObject as Actor.cs that have IActorComponent will have SaveData / LoadData called when Actor.cs Load is called.



* IActorComponent interface should be on all GameObject components that need to be saved, loaded see Jaakko/Scripts/Health/HealthComponent.cs for usage. Actor Components can decide which data the actor should save



#### **EXAMPLES:**

* See JaakkoScene -> Actor\_Player gameObject and its scripts. This obj has components and is ready to be saved / loaded as well as having dependencies injected properly
* See Jaakko/Scripts/Examples/InputExample.cs for proper dependency gathering types











