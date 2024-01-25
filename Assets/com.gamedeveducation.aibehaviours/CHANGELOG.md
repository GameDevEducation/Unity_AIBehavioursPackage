## [3.0.2] - 26 January 2023
 - Fixes #1 - If there are no valid interactables return float.MinValue rather than the default (shoutout to SiliconOrchid for finding the issue)

## [3.0.1] - 7 January 2023
 - Code cleanup pass

## [3.0.0] - 7 January 2023
 - Added remaining components for the new perception system
 - Removed legacy perception system

## [2.2.0] - 7 January 2023
 - Work in progress implementation for Perception System (not yet in use)

## [2.1.0] - 7 January 2023
 - Add SerializableType (a version of System.Type) that can be serialized and used in the inspector

## [2.0.1] - 6 January 2023
 - Switch Name Manager over to Standalone Singleton and enable bootstrapping of it
 - Make the Blackboard Manager a locatable service and enable registering of it
 - Minor commenting additions in GOAPActionBase

## [2.0.0] - 6 January 2023
 - Added Service Locator and Standalone Singleton
 - Removed GOAP Navigation Interface (replaced by service lookup)
 - Renamed Singleton to MonoBehaviourSingleton

## [1.0.6] - 4 January 2023
 - Fix GOAPGoal_GatherResource failing on shutdown
 - Run a code cleanup to remove unused usings and enforce consistency
 - Add all sample scenes to the build

## [1.0.5] - 19 December 2023
 - Remove TextMesh Pro dependency (needs manual install for pre 2023.2)

## [1.0.4] - 19 December 2023
 - Fix unintended include of Visual Scripting

## [1.0.3] - 2 December 2023
 - Add support to the navigation system for using the agent locomotion except in the start/end orienting states
 - Enable the new behaviour by default
 - Increase the number of agents spawned in the village to 20

## [1.0.2] - 2 December 2023
 - Provide a cleaner interface for adding states to the state machine
 - Provide a cleaner interface for adding state status transition and default transitions

## [1.0.1] - 23 November 2023
 - Remove incorrect using statement in SMState_Store

## [1.0.0] - 23 November 2023
 - Initial Version