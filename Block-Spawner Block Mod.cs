using System;
using System.Collections;
using System.Collections.Generic;
using spaar.ModLoader;
using TheGuysYouDespise;
using UnityEngine;

namespace Blocks
{
    public class BlockSpawnerBlockMod : BlockMod
    {
        public override Version Version { get { return new Version("1.2"); } }
        public override string Name { get { return "BlockSpawnerBlockMod)"; } }
        public override string DisplayName { get { return "Block-Spawner Block Mod"; } }
        public override string BesiegeVersion { get { return "v0.25"; } }
        public override string Author { get { return "覅是"; } }
        protected Block blockSpawnerBlock = new Block()
            .ID(519)
            .BlockName("Block-Spawner Block")
            .Obj(new List<Obj> { new Obj("BlockSpawnerBlock.obj", //Obj
                                         "BlockSpawnerBlockTexture.png", //贴图
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(0f, 0f, 0f), //Position
                                                          new Vector3(0f, 0f, 0f)))//Rotation
            })
            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(new Vector3(1.30f, 1.30f, 1.30f),  //Scale
                                 new Vector3(-0.11f, -0.13f, 0.00f),  //Position
                                 new Vector3(45f, 45f, 45f))) //Rotation
            .Components(new Type[] {typeof(blockSpawnerBlockS),})

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Block",
                                                             "模块生成模块",
                                                             "Spawner",
                                             }))
            .Mass(0.5f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> {new ColliderComposite(new Vector3(1, 1, 1), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f))})
            .NeededResources(new List<NeededResource> {
                                new NeededResource(ResourceType.Audio,("paaaa.ogg"))
            })
            .AddingPoints(new List<AddingPoint> {
                               (AddingPoint)new BasePoint(true, true)
                                                .Motionable(false,false,false)
                                                .SetStickyRadius(0.5f),
            });

        public override void OnLoad()
        {
            LoadBlock(blockSpawnerBlock);//加载该模块
        }
        public override void OnUnload() { }
    }


    public class blockSpawnerBlockS : BlockScript
    {
        protected MKey Key1;
        protected MSlider 模块ID;
        protected MSlider 生成间隔;
        protected MSlider 生成大小;
        protected MToggle 继承速度;
        private int sliderValve;
        private AudioSource Audio;
        private int countdown;


        public override void SafeAwake()
        {
            Key1 = AddKey("Spawn block", //按键信息
                                 "Spawn",           //名字
                                 KeyCode.V);       //默认按键

            模块ID = AddSlider("Block ID",       //滑条信息
                                    "ID",       //名字
                                   23.5f,            //默认值
                                    0f,          //最小值
                                    57f);           //最大值

            生成间隔 = AddSlider("Spawn Frequency",       //滑条信息
                                    "Freq",       //名字
                                   0.25f,            //默认值
                                    0f,          //最小值
                                    10f);           //最大值

            生成大小 = AddSlider("Block Scale",       //滑条信息
                                    "Scale",       //名字
                                    1f,            //默认值
                                    0.01f,          //最小值
                                    100f);           //最大值

            继承速度 = AddToggle("Inherit My Velocity",   //toggle信息
                                       "IMV",       //名字
                                       true);             //默认状态
        }

        protected virtual IEnumerator UpdateMapper()
        {
            if (BlockMapper.CurrentInstance == null)
                yield break;
            while (Input.GetMouseButton(0))
                yield return null;
            BlockMapper.CurrentInstance.Copy();
            BlockMapper.CurrentInstance.Paste();
            yield break;
        }
        public override void OnSave(BlockXDataHolder data)
        {
            SaveMapperValues(data);
        }
        public override void OnLoad(BlockXDataHolder data)
        {
            LoadMapperValues(data);
            if (data.WasSimulationStarted) return;
        }

        protected override void OnSimulateStart()
        {
            sliderValve = (int)模块ID.Value;
            Audio = this.gameObject.AddComponent<AudioSource>();
            Audio.clip = resources["paaaa.ogg"].audioClip;
            Audio.loop = false;
            Audio.volume = 70;
            countdown = 0;

        }
        protected override void OnSimulateFixedUpdate()
        {
            if (countdown > 0)
            {
                countdown -= 1;
            }
            if (AddPiece.isSimulating)
            {
                if (Key1.IsDown && countdown == 0)
                {
                    GameObject Nlock = (GameObject)UnityEngine.Object.Instantiate(Game.AddPiece.blockTypes[sliderValve].gameObject, this.transform.position + this.transform.forward, this.transform.rotation);
                    Destroy(Nlock.GetComponent<HighlightController>());
                    Nlock.transform.localScale *= 生成大小.Value;
                    Nlock.GetComponent<Rigidbody>().isKinematic = false;
                    if (继承速度.IsActive) { Nlock.GetComponent<Rigidbody>().velocity = this.rigidbody.velocity; }
                    Nlock.transform.SetParent(Machine.Active().SimulationMachine);

                    Audio.Play();
                    float ctdTemp = 生成间隔.Value * 100;
                        countdown = (int)ctdTemp ;
                }

            }
            //Physics stuff

        }
    }


}
