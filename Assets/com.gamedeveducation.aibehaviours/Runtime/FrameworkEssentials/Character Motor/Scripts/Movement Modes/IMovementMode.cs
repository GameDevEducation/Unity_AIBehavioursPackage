using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementMode
{
    void Initialise(CharacterMotorConfig config, CharacterMotor motor, CharacterMotor.MotorState state);

    void FixedUpdate_PreGroundedCheck();

    RaycastHit FixedUpdate_GroundedCheck();

    void FixedUpdate_OnBecameGrounded();

    void FixedUpdate_TickMovement(RaycastHit groundCheckResult);

    void LateUpdate_Tick();

    float CurrentMaxSpeed { get; set; }
}
