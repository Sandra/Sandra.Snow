# Sandra.Snow

Sandra.Snow is a Jekyll inspired static site generation tool that can be setup with Azure to build your site when your repository changes. 

## Notes

This is the initial commit which contains a very very rough messy cut of code that works against my current blog which runs on Github pages using Jekyll.

Included is 'SnowSite' which Visual Studio will build against and generate a Website folder containing all of the compiled site.

The code contains a lot of hard-coded values, duplication, clutter, etc. It's basically a hacked together solution that I got up and running and now that I've put this on Github I intend to refactor it and make this more usable for everyone.

Feel free to help out!

<https://vimeo.com/65055971>

This video shows a really rough showcase of setting up an Azure website and deploying the website which gets compiled.

To get the F5 build working you will need to open up the project properties, and change the Debug command line arguments to specify the root directory of the SnowSite folder. 