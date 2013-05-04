# Sandra.Snow

Sandra.Snow is a Jekyll inspired static site generation tool that can be setup with Azure to build your site when your repository changes. 

## Notes

This is the initial commit which contains a very very rough messy cut of code that works against my current blog which runs on Github pages using Jekyll.

Included is 'SnowSite' which Visual Studio will build against and generate a Website folder containing all of the compiled site.

The code contains a lot of hard-coded values, duplication, clutter, etc. It's basically a hacked together solution that I got up and running and now that I've put this on Github I intend to refactor it and make this more usable for everyone.

Feel free to help out!

<https://vimeo.com/65055971>

This video shows a really rough showcase of setting up an Azure website and deploying the website which gets compiled.

## How to run the project

To run the project when you clone the repository. Open up the Project Properties and go to the Debug tab. For the Startup Options there should be a argument called `config=` this should be the full path to the SnowSite folder. If not then add it.

The site should compile when you hit F5 now. 

## sandra.Snow.Barbato

This is a web application that can be hosted on a server that will do the static generation rather than locally.  It works by accepting a Github web hook, pulling your Git changes, genererating the content, moving the output to a location of choice and then pushing the Git repo changes back to Github.