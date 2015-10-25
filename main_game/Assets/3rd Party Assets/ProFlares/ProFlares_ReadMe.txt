
|————————————————————————————————————————————————————|

ProFlares - v1.0

Thank you for purchasing ProFlares, I hope you enjoy using it. By supporting ProFlares you allow me to spend time supporting ProFlares and adding more features and content for free. :)

If you have any queries or issues please do not hesitate to contact support. Please include your invoice number. - proflaresunity@gmail.com

Full Documentation and support can be found at www.proflares.com

Have you created something awesome using ProFlares and want to share it? I can post it in the gallery section of my website with links to your game.


|————————————————————————————————————————————————————|

 - Getting Started - Demos scenes. 

Included in ProFlares are four demos scenes

IslandDemo - This shows of a number of real world use cases for ProFlares.

Use the on screen controls to change between setups, click dragging to pan the camera and use the mouse wheel to zoom.

The scene contains three distinct setups, the first being a day time sun, a spooky night time setup and an overcast day setup.

The day setup uses one sun flare with a number of elements to create a natural flare. This flare is a great example of the Dynamic triggering feature.

In the night setup there are three flares, which are all rendered in one draw call. Also an additional flare is parented to the camera and is used to give an off screen glow effect.

The overcast setup uses two flares, the first being the main sun flare which is occluded by the trees. The second is a soft glow that isn’t occluded by the trees.

All flares in this scene use the IslandDemoAtlas.


Preset Preview -

This scene is a great way to preview the example flares that ship with ProFlares, use the arrow keys to cycle through the flares and click drag anywhere on screen to move the flare. Try positioning the flare just of screen to see how the dynamic triggering feature causes flares to get brighter just as they leave the screen.

All flares in this scene use the MegaAtlas.


Multiple Camera Demo - 

This scene demonstrates how to setup ProFlares to work with multiple cameras. The switching of cameras is handled by MultiCameraDemo.cs script.

Split Screen Demo - 

This demo shows how to use pro flares in a split screen environment. Each flare is rendered by two ProFlareBatch setups, one for each camera perspective.


|————————————————————————————————————————————————————|


- Understanding ProFlare atlases -

ProFlares is very efficient at rendering flares, one of the reasons for this as it utilises ‘Texture Atlases’.

A texture atlas is a texture that contains multiple images of individual elements.

ProFlares ships with three atlases, BasicAtlas, MegaAtlas and an additional Atlas for the IslandDemo Scene.

The basic atlas contains the essential building blocks of many types of lens flares, but only contains a small number of elements from the library. For more advanced flares consider working with the mega atlas.

The mega atlas is much larger and contains more varied flare elements. Using this atlas is great for testing out ideas, but you may find that your only using a small number of elements from the atlas. This will be waste full in texture memory especially on mobile. In this case you may wish to create an new atlas that is only using the flare elements that are used by your flare. If you use the same elements names in your new atlas you can swap out the mega atlas for your new one without any issues.

You can mix multiple atlases in the same scene. You will just need to have an additional ProFlareBatch in your scene, e.g. one for each atlas used.

For more information on using and creating texture atlas go to ProFlares.com

|————————————————————————————————————————————————————|

 - Accessing the Elements Library -

For convenience I have included the ‘ProFlares Element Library’ as a zip file. As it contains over 120 large images and can take a while to import directly into unity. It is recommend that you move the file before unzipping it to avoid unnecessarily long import times.


For more tutorials and documentation please visit www.ProFlares.com