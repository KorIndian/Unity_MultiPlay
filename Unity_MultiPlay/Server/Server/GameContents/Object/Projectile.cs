using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class Projectile : GameObject
    {
        public SkillData skillData { get; set; }

        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
