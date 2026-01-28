using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace Maurinaattori;

/// @author gr313123
/// @version 19.01.2026
/// <summary>
/// 
/// </summary>
public class Maurinaattori : PhysicsGame
{
    private PhysicsObject targetball;
    private PhysicsObject lahnaPallo;
    private PhysicsObject[] pallot;
    
    public override void Begin()
    {
        SetWindowSize(800, 600);
        Level.Size = new Vector(800, 600);
        pallot = TeeSatunnaisetPallot(20, 30.0);

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, lahnaaPallo, "Lahnaa pallon");

        targetball = new PhysicsObject(30.0, 30.0);
        targetball.Shape = Shape.Circle;
        targetball.Color = Color.Blue;
        Add(targetball);

        LahinPallo(pallot, targetball.Position);

    }

    private void lahnaaPallo()
    {
        targetball.Position = Mouse.PositionOnWorld;
        LahinPallo(pallot, targetball.Position);
    }

    /// <summary>
    /// Tekee palloja satunnaisiin paikkoihin
    /// </summary>
    /// <param name="montako">Montako palloa tehdään</param>
    /// <param name="koko">Pallon koko</param>
    /// <returns>Pallot</returns>
    private PhysicsObject[] TeeSatunnaisetPallot(int montako, double koko)
    {
        PhysicsObject[] pallot = new PhysicsObject[montako];
        for (int i = 0; i < montako; i++)
        {
            PhysicsObject p = new PhysicsObject(koko, koko, Shape.Circle);
            pallot[i] = p;
            p.Position = RandomGen.NextVector(Level.BoundingRect);
            Add(p);
        }
        return pallot;
    }

    public PhysicsObject LahinPallo(PhysicsObject[] pallot, Vector piste)
    {
        PhysicsObject lahnaPallo = pallot[0];
        
        foreach (PhysicsObject lahna in pallot)
        {
            lahna.Color = Color.White;
            if (lahna.Position.Distance(piste) < lahnaPallo.Position.Distance(piste))
            {
                lahnaPallo = lahna;
            }
        }

        lahnaPallo.Color = Color.Red;
        return lahnaPallo;
    }
}