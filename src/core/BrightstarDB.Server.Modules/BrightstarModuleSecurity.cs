﻿using BrightstarDB.Server.Modules.Permissions;
using Nancy;

namespace BrightstarDB.Server.Modules
{
    public static class BrightstarModuleSecurity
    {
        public static void RequiresBrightstarStorePermission(this NancyModule module,
            AbstractStorePermissionsProvider permissionsProvider, StorePermissions get = StorePermissions.None, StorePermissions post = StorePermissions.None, StorePermissions delete = StorePermissions.None, StorePermissions put = StorePermissions.None)
        {
            module.Before.AddItemToEndOfPipeline(ctx => RequiresAuthorization(ctx, permissionsProvider, get, post, delete, put));
        }

        /// <summary>
        /// Extension method to request that the NancyContext ViewBag gets populate
        /// with the store permissions during the Before module pipeline.
        /// </summary>
        /// <param name="module">The module requesting permission data</param>
        /// <param name="permissionsProvider">The provider that retrieves permission info for the current user</param>
        public static void RequiresBrightstarStorePermissionData(this NancyModule module, AbstractStorePermissionsProvider permissionsProvider)
        {
            module.Before.AddItemToEndOfPipeline(ctx=>RequiresPermissionData(ctx,permissionsProvider));
        }

        public static void RequiresBrightstarSystemPermission(this NancyModule module,
                                                              AbstractSystemPermissionsProvider permissionsProvider,
                                                              SystemPermissions get = SystemPermissions.None,
                                                              SystemPermissions post = SystemPermissions.None)
        {
            module.Before.AddItemToEndOfPipeline(ctx=>RequiresAuthorization(ctx, permissionsProvider, get, post));
        }

        private static Response RequiresPermissionData(NancyContext context,
                                                       AbstractStorePermissionsProvider permissionsProvider)
        {
            var storeName = context.Parameters["storeName"];
            context.ViewBag["BrightstarStorePermissions"] = permissionsProvider.GetStorePermissions(context.CurrentUser,
                                                                                                   storeName);
            return null;
        }

        private static Response RequiresAuthorization(NancyContext context, AbstractStorePermissionsProvider permissionsProvider, StorePermissions get, StorePermissions post, StorePermissions delete, StorePermissions put)
        {
            var permissionRequired = StorePermissions.None;
            
            switch (context.Request.Method.ToUpperInvariant())
            {
                case "GET":
                case "HEAD":
                    permissionRequired = get;
                    break;

                case "POST":
                    permissionRequired = post;
                    break;

                case "PUT":
                    permissionRequired = put;
                    break;

                case "DELETE":
                    permissionRequired = delete;
                    break;
            }

            if (permissionRequired == StorePermissions.None) return HttpStatusCode.Unauthorized;

            var storeName = context.Parameters["storeName"];
            if (!permissionsProvider.HasStorePermission(context.CurrentUser, storeName, permissionRequired))
            {
                return HttpStatusCode.Unauthorized;
            }
            return null;
        }

        private static Response RequiresAuthorization(NancyContext context,
                                                      AbstractSystemPermissionsProvider permissionsProvider,
                                                      SystemPermissions get, SystemPermissions post)
        {
            var permissionsRequired = SystemPermissions.None;
            switch (context.Request.Method.ToUpperInvariant())
            {
                case "GET":
                case "HEAD":
                    permissionsRequired = get;
                    break;
                case "POST":
                    permissionsRequired = post;
                    break;
            }
            if (permissionsRequired == SystemPermissions.None) return HttpStatusCode.Unauthorized;

            if (!permissionsProvider.HasPermissions(context.CurrentUser, permissionsRequired))
            {
                return HttpStatusCode.Unauthorized;
            }
            return null;
        }

    }
}
