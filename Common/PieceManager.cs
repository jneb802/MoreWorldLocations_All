using System.Collections.Generic;
using Jotunn.Entities;

namespace Common
{
    public class PieceManager
    {
        public static void ReplaceResourceDrops(CustomLocation location)
        {
            Piece[] allPieces = location.Prefab.GetComponentsInChildren<Piece>();
            
            foreach (var piece in allPieces)
            {
                if (piece.transform.parent != null)
                {
                    WarpLogger.Logger.LogDebug("Piece with name " + piece + " found in location with name: " + location);

                    var resources = piece.m_resources;

                    foreach (var resource in resources)
                    {
                        if (resource != null)
                        {
                            WarpLogger.Logger.LogDebug("Resource with name " + resource + " found in piece  with name: " + piece.transform.parent.name);
                        }
                    }
                }
            }
            
            
                
        }
    }
}