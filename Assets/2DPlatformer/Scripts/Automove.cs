using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automove : RetroPhysicsObject {

    private void Update()
    {
        targetVelocity = Vector2.left;
    }
}
