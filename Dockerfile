# PhotoGallery.Bff/Dockerfile

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


# ---------- 2. Build PhotoGallery.Bff ----------
# Copy solution and project files for restore caching
COPY PhotoGallery.Bff/PhotoGallery.Bff.sln PhotoGallery.Bff/
COPY PhotoGallery.Bff/PhotoGallery.Bff.Api/PhotoGallery.Bff.Api.csproj PhotoGallery.Bff/PhotoGallery.Bff.Api/

# Restoring API
RUN dotnet restore PhotoGallery.Bff/PhotoGallery.Bff.Api/PhotoGallery.Bff.Api.csproj

# Copy rest of source
COPY PhotoGallery.Bff/PhotoGallery.Bff.Api/ PhotoGallery.Bff/PhotoGallery.Bff.Api/

# Publish
RUN dotnet publish PhotoGallery.Bff/PhotoGallery.Bff.Api/PhotoGallery.Bff.Api.csproj -c Release -o /app --no-restore


# ---------- runtime stage ----------
# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# 1. Add a non-root user named 'appuser'
RUN adduser --disabled-password --gecos "" --no-create-home appuser

WORKDIR /app

# 2. Change ownership of the /app directory to the new user.
RUN chown -R appuser:appuser /app

# 3. Copy published output
COPY --from=build /app ./

# 4. Expose the port.
EXPOSE 8080

# 5. Switch to the non-root user
USER appuser

# 6. Set the final entrypoint
ENTRYPOINT ["dotnet", "PhotoGallery.Bff.Api.dll"]