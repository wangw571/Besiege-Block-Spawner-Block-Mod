using System;
using System.Collections.Generic;
using spaar.ModLoader;
using TheGuysYouDespise;
using UnityEngine;

namespace Blocks
{
    public class BlockSpawnerBlockMod : BlockMod
    {
        public override Version Version { get { return new Version("1.1"); } }
        public override string Name { get { return "BlockSpawnerBlockMod)"; } }
        public override string DisplayName { get { return "Block-Spawner Block Mod"; } }
        public override string BesiegeVersion { get { return "v0.11"; } }
        public override string Author { get { return "覅是"; } }
        protected Block blockSpawnerBlock = new Block()
                .ID(519)
                .TextureFile("BlockSpawnerBlockTexture.png")
                .BlockName("Block-Spawner Block")
                .Obj(new List<Obj> { new Obj("BlockSpawnerBlock.obj", new VisualOffset(Vector3.one, Vector3.zero, Vector3.zero)) })
                .Scripts(new Type[] { typeof(blockSpawnerBlockS) })
                .Properties(new BlockProperties().KeyBinding("Spawn Block", "v")
                                                 .CanBeDamaged(Mathf.Infinity)
                                                 .Slider("Block ID", 0, 55, 23)
                                                 )
                .Mass(0.5f)
                .IconOffset(new Icon(1f, new Vector3(0f, 0f, 0f), new Vector3(-90f, 45f, 0f)))//第一个float是图标缩放，五六七是我找的比较好的角度
                .ShowCollider(false)
                .AddingPoints(new List<AddingPoint> { new BasePoint(true, true) })
                .CompoundCollider(new List<ColliderComposite> {new ColliderComposite(new Vector3(1,1,1), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f)) })
                .NeededResources(new List<NeededResource> { new NeededResource(ResourceType.Audio, "paaaa.ogg") }//需要的资源，例如音乐

            );
        public override void OnLoad()
        {
            LoadFancyBlock(blockSpawnerBlock);//加载该模块
        }
        public override void OnUnload() { }
    }


    public class blockSpawnerBlockS : BlockScript
    {
        private string key1;
        private string key2;
        private int sliderValve;
        private AudioSource Audio;
        private int countdown;




        protected override void OnSimulateStart()
        {

            key1 = this.GetComponent<MyBlockInfo>().key1;
            key2 = this.GetComponent<MyBlockInfo>().key2;
            sliderValve = (int)this.GetComponent<MyBlockInfo>().sliderValue;

            Audio = this.gameObject.AddComponent<AudioSource>();
            Audio.clip = new WWW("File:///" + Application.dataPath + "/Mods/Blocks/Resources/paaaa.ogg").audioClip;
            Audio.loop = false;
            countdown = 0;

        }
        protected override void OnSimulateFixedUpdate()
        {if (countdown > 0)
            {
                countdown -= 1;
            }
            if (AddPiece.isSimulating)
            {
                if (Input.GetKey(key1) && countdown == 0)
                {
                        GameObject component = (GameObject)UnityEngine.Object.Instantiate(Game.AddPiece.blockTypes[sliderValve].gameObject, this.transform.position + this.transform.forward, this.transform.rotation);
                        component.rigidbody.isKinematic = false;
                    component.transform.parent = this.transform.parent;
                    Audio.Play();
                    countdown = 25;
                }

            }
            //Physics stuff

        }
    }


}
