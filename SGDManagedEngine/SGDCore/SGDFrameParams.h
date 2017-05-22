#pragma once
// data for each displayed frame
//	- one instance for each new frame to eventually be displayed
//	- set through all stages of the engine
// contains per-frame state
//	- frame number
//	- delta time
//	- skinning matrices
// entry point for each stage to access required data
// un-contended resource
//	- no locks required as each stage works on a unique instance
// state variables are copied into this structure every frame
//	- delta time
//	- camera position
//	- skinning matrices
// stores start/end timestamps for each stage
// easy to test if a frame has completed a particular stage
//	- HasFrameCompleted(frameNumber)
// memory lifetime is now easily tracked
//	- if you generate data to be consumed by the GPU in frame X, then you wait until HasFrameCompleted(X) is true
// you have 16 FrameParams that we rotate between
//	- you can only track the state of the last 15 frames
// memory life time
//	- single game logic stage (scratch memory)
//	- double game logic stage (low-priority ray casts)
//	- game to render logic stage (object instance arrays)
//	- game to GPU stage (skinning matrices)
//	- render to GPU stage (command buffers)
//	- ... for both Onion(CPU) and Garlic(GPU) memory!
