# Note for Simon

After downloading the solution, you should be able to just press F5 to run it. 

To debug the exception (it's not easy at the moment :() but just put a break point inside:

Extensions > StatusCodeExtension > ThrowIfNotSuccessful

On the if statement, and have a look at the Nancy response.

# Sandra.Snow

Sandra.Snow is a Jekyll inspired static site generation tool that can be run locally, as a CAAS(Compiler as a Service) or setup with Azure to build your site when your repository changes. It is built on top of [NancyFX][1].

## Notes

Included is 'SnowSite' which Visual Studio will build against and generate a Website folder containing all of the compiled site.

Feel free to help out!

<https://vimeo.com/65055971>

This video shows a really rough showcase of setting up an Azure website and deploying the website which gets compiled.

## How to run the project

To run the project when you clone the repository, open up the Project Properties in VS and go to the Debug tab. For the Startup Options there should be a argument called `config=` this should be the full path to the SnowSite folder. If not then add it.

The site should compile when you hit F5 now. 

## Sandra.Snow.Barbato

This is a web application that can be hosted on a server that will do the static generation rather than locally.  It works by accepting a Github web hook, pulling your Git changes, genererating the content, moving the output to a location of choice and then pushing the Git repo changes back to Github.

[1]: https://github.com/NancyFx/Nancy
