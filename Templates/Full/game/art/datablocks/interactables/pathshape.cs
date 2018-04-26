//********************************************
// Looping path shape
// exec in game.cs as follows:
// exec("./pathShape.cs");
// in editor we need a path with a few markers. Name it as you wish.
// Then in runtime use: platformID.followPath("MyPathName");
// And jump over the platform,that already moves across the markers.
//********************************************


// this is an example of how to trigger the pathshape to move. you can do it
// anyway you like, i simply decided to trigger it's movement on a collision, so a jump on it
// may be required to make it "go"
// The state must be set to make it function. "forward" "back" or "stopped" are the three
// states a pathshape can be in.

datablock PathShapeData(LoopingShape)
{
//This is the Path this shape will follow
// It's best to assign this in script or in the mission editor after it's been created
// in the oncollision, it will look for this path
   Path="MissionGroup/mPath";
   emap = true;
   category = "Pathshapes";

   // put path to your shape here!!
   shapeFile = "art/shapes/rocks/boulder.dts";
   mode = "";
};


function LoopingShape::onCollision(%this,%obj,%col){
    //error("A collsion with a loopingshape has occurred");
	%state = %obj.getState();
	if (%state == 2)  // 2 == stopped.
	{
		%obj.setState("forward");
		%obj.followpath(%obj.Path);
	}
}

function LoopingShape::onAdd(%this,%obj)
{
   // do whatever you want with that shape, e.g. play animations:
   %obj.schedule(300, "assignPath", %obj.Path);
   %obj.canSaveDynamicFields = false;
}

function PathShape::assignPath(%this,%path)
{
   if(!isObject(%path)) return;
   %this.Path = %path;
   if (isObject(%this.Path))
   {
		%this.reset();
		%this.followPath(%this.Path);
        %this.setState("stop");
		%this.setPosition(0);
   }
}

function LoopingShape::onNode(%this,%theshape,%node) {
   //echo("PathShape::onNode(" @ %this @ "," SPC %theshape @ "," SPC %node@")");
   if(%theshape.path.isLooping && (%node >= %theshape.loopNode))
   {
       //error("At end of path, setting position back to starting node.");
	   %theshape.setPosition(0);
   }
}

function PathShape::followPath(%this,%path)
{
   if(!isObject(%path)) return;
   %this.path = %path;
   if (%this.speed != %path.speed)
      %this.speed = %path.speed;

   if (%path.isLooping) {
      %this.loopNode = %path.getCount();  
   } else {
      %this.loopNode = -1;
   }
   
   %this.pushPath(%path);   
   %this.popFront(); // This pop removes initial DUMMY node.
}

function PathShape::pushPath(%this,%path)
{
   if(!isObject(%path)) return;
   for (%i = 0; %i < %path.getCount(); %i++)
   {
      %this.pushNode(%path.getObject(%i));
   }
   // If looping, push the starting node on to end to make this a loop
   if(%this.path.isLooping)
   {
       %this.pushNode(%path.getObject(0));
   }
}
function PathShape::pushNode(%this,%node)
{
   if(!isobject(%node)) return;
   //node type safety
   if ((%type = %node.type) !$= "Normal")
      if ((%type = %node.type) !$= "Kink")
         %type = "Position Only";
      
   //smoothing type safety
   if ((%smoothing = %node.smoothingType) !$= "SPLINE")
      %smoothing = "Linear";
  
   %this.pushBack(%node.getTransform(),%this.speed/1000,%type,%smoothing);
}
//-----------------------------------------------------------------------------------
datablock PathShapeData(AutoLoopingShape : LoopingShape)
{
   //This is the Path this shape will follow
   // It's best to assign this in script or in the mission editor after it's been created
   // in the oncollision, it will look for this path
   category = "Pathshapes";

   // put path to your shape here!!
   shapeFile = "art/shapes/rocks/boulder.dts";
   mode = "";
};

function AutoLoopingShape::onAdd(%this,%obj)
{
   // do whatever you want with that shape, e.g. play animations:
   //%obj.Path = %this.Path;
   %this.schedule(300, "startLooping",%obj);
   %obj.canSaveDynamicFields = false;
}

function AutoLoopingShape::startLooping(%this,%obj)
{
	if ($Game::Running != true)
		%this.schedule(300, "startLooping",%obj);
	else
	{		
		%obj.assignPath(%obj.Path);
		%obj.setState("forward");
	}
}

function AutoLoopingShape::onNode(%this,%theshape,%node)
{
   //echo("PathShape::onNode(" @ %this @ "," SPC %theshape @ "," SPC %node@")");
   if(%theshape.path.isLooping && (%node == %theshape.loopNode)) {
       //error("At end of path, setting position back to starting node.");
	   %theshape.setPosition(0);
   }
}
//aaabridge.moveForward();aaabridge.moveBackward();
function PathShape::moveForward(%this)
{
	%this.reset();
	%this.followPath(%this.Path);
	%this.setState("forward");
	%this.playThread( 0, "forward" );
	%this.setThreadDir(0,true);
	%this.schedule(1000,"stopThread",0);
}

function PathShape::moveBackward(%this)
{ 
	%this.reset();
	%this.followPath(%this.Path);
	%this.setPosition(%this.path.getCount());
	%this.setState("backward");
	%this.playThread( 0, "forward" );
	%this.setThreadDir(0,false);
	%this.schedule(1000,"stopThread",0);
}