using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class BlockSpawnerBlockMod : BlockMod
    {
        public override Version Version { get { return new Version("2.0"); } }
        public override string Name { get { return "BlockSpawnerBlockMod)"; } }
        public override string DisplayName { get { return "Block-Spawner Block Mod"; } }
        public override string BesiegeVersion { get { return "v0.32"; } }
        public override string Author { get { return "覅是 (Thanks ITR for Block Mapper Support!)"; } }
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
            .Components(new Type[] { typeof(blockSpawnerBlockS), })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Block",
                                                             "模块生成模块",
                                                             "Spawner",
                                             }))
            .Mass(0.5f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(1, 1, 1), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f)) })
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
        protected MToggle FunnyMode;
        protected MToggle modifyBlock;
        protected MMenu 模块ID;
        protected MSlider 生成间隔;
        protected MSlider 生成大小;
        protected MToggle 继承速度;
        private AudioSource Audio;

        private List<int> 对应的IDs = new List<int>();
        private float countdown;
        public BlockBehaviour blockToSpawn;
        private Dictionary<int, BlockPrefab>.Enumerator funEnumerator;

        public override void SafeAwake()
        {
            Key1 = AddKey("Spawn block", //按键信息
                                 "Spawn",           //名字
                                 KeyCode.V);       //默认按键

            List<string> list = new List<string>();
            foreach (BlockPrefab pair in PrefabMaster.BlockPrefabs.Values)
            {
                list.Add(pair.gameObject.GetComponent<MyBlockInfo>().blockName);
                对应的IDs.Add(pair.ID);
            }
            模块ID = AddMenu("Block", 0, list);
            模块ID.ValueChanged += IDChanged;

            modifyBlock = AddToggle("Modify Spawned Block", "modifySpawnedBlock", false);
            modifyBlock.Toggled += (b) => {
                if (b)
                {
                    SpawnChild();
                    modifyBlock.IsActive = false;
                    BlockMapper.Open(blockToSpawn);
                }
            };

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
            FunnyMode = AddToggle("Funny Mode",   //toggle信息
                                       "FMD",       //名字
                                       false);             //默认状态
            FunnyMode.Toggled += (t) => {
                modifyBlock.DisplayInMapper = !t;
            };


        }

        private void SpawnChild()
        {
            if (blockToSpawn != null)
            {
                if (blockToSpawn.GetBlockID() == 模块ID.Value)
                {
                    return;
                }
                Destroy(blockToSpawn.gameObject);
            }
            if (!FunnyMode.IsActive)
            {
                blockToSpawn = Instantiate(PrefabMaster.BlockPrefabs[对应的IDs[模块ID.Value]].blockBehaviour);
                blockToSpawn.gameObject.SetActive(false);
                //blockToSpawn.transform.SetParent(this.transform);
            }
            else
            {
                funEnumerator = PrefabMaster.BlockPrefabs.GetEnumerator();
                while (funEnumerator.MoveNext())
                {
                    if (funEnumerator.Current.Value.ID == 对应的IDs[模块ID.Value])
                    {
                        return;
                    }
                }
                funEnumerator = PrefabMaster.BlockPrefabs.GetEnumerator();
                funEnumerator.MoveNext();
            }
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
        public override void OnSave(XDataHolder data)
        {
            SpawnChild();
            SaveMapperValues(data);
            if (blockToSpawn != null)
            {
                blockToSpawn.OnSave(data);
            }
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            if (blockToSpawn == null && !FunnyMode.IsActive)
            {
                SpawnChild();
            }
            if (blockToSpawn != null && !StatMaster.isSimulating)
            {
                blockToSpawn.gameObject.SetActive(true);
                blockToSpawn.OnLoad(data);
                blockToSpawn.gameObject.SetActive(false);
            }
        }

        protected override void OnSimulateStart()
        {
            Audio = gameObject.AddComponent<AudioSource>();
            Audio.clip = resources["paaaa.ogg"].audioClip;
            Audio.loop = false;
            Audio.volume = 0.01f;
            countdown = 0;
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (countdown > 0)
            {
                countdown -= Time.fixedDeltaTime;
            }
            if (Key1.IsDown && countdown <= 0)
            {
                GameObject Nlock;
                if (FunnyMode.IsActive)
                {
                    Nlock = (GameObject)Instantiate(funEnumerator.Current.Value.gameObject,
                        this.transform.position + this.transform.forward, this.transform.rotation);
                    if (!funEnumerator.MoveNext())
                    {
                        funEnumerator = PrefabMaster.BlockPrefabs.GetEnumerator();
                        funEnumerator.MoveNext();
                    }
                }
                else
                {
                    if (blockToSpawn == null)
                    {
                        SpawnChild();
                    }
                    Nlock = (GameObject)Instantiate(blockToSpawn.gameObject,
                    transform.position + this.transform.forward, this.transform.rotation);
                    Nlock.SetActive(true);
                    XDataHolder xDataHolder = new XDataHolder { WasSimulationStarted = true };
                    blockToSpawn.OnSave(xDataHolder);
                    Nlock.GetComponent<BlockBehaviour>().OnLoad(xDataHolder);
                }
                Nlock.transform.localScale *= 生成大小.Value;
                Nlock.GetComponent<Rigidbody>().isKinematic = false;
                if (继承速度.IsActive) { Nlock.GetComponent<Rigidbody>().velocity = this.rigidbody.velocity; }
                Nlock.transform.SetParent(Machine.Active().SimulationMachine);
                Audio.volume = 0.05f * 10 / Vector3.Distance(this.transform.position, Camera.main.transform.position);
                Audio.Play();
                countdown = 生成间隔.Value;
            }
        }
        //Physics stuff
        public void IDChanged(int ID)
        {
            if (BlockMapper.CurrentInstance == null || BlockMapper.CurrentInstance.Block != this)
            {
                SpawnChild();
            }
        }

        public override void OnReset()
        {
            base.OnReset();
            SpawnChild();
        }

        //protected void OnDestroy()
        //{
        //    if (blockToSpawn != null)
        //    {
        //        Destroy(blockToSpawn.gameObject);
        //    }
        //}
    }
}
