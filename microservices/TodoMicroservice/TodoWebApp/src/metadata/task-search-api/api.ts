/* tslint:disable */
/* eslint-disable */
/**
 * SearchApi
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 1.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */


import { Configuration } from './configuration';
import globalAxios, { AxiosPromise, AxiosInstance, AxiosRequestConfig } from 'axios';
// Some imports not used depending on template conditions
// @ts-ignore
import { DUMMY_BASE_URL, assertParamExists, setApiKeyToObject, setBasicAuthToObject, setBearerAuthToObject, setOAuthToObject, setSearchParams, serializeDataIfNeeded, toPathString, createRequestFunction } from './common';
// @ts-ignore
import { BASE_PATH, COLLECTION_FORMATS, RequestArgs, BaseAPI, RequiredError } from './base';

/**
 * 
 * @export
 * @interface TaskSearchResultViewModel
 */
export interface TaskSearchResultViewModel {
    /**
     * 
     * @type {number}
     * @memberof TaskSearchResultViewModel
     */
    'id'?: number;
    /**
     * 
     * @type {string}
     * @memberof TaskSearchResultViewModel
     */
    'title'?: string | null;
    /**
     * 
     * @type {boolean}
     * @memberof TaskSearchResultViewModel
     */
    'completed'?: boolean;
}
/**
 * 
 * @export
 * @interface TaskSearchViewModel
 */
export interface TaskSearchViewModel {
    /**
     * 
     * @type {string}
     * @memberof TaskSearchViewModel
     */
    'text'?: string | null;
    /**
     * 
     * @type {boolean}
     * @memberof TaskSearchViewModel
     */
    'completed'?: boolean | null;
}

/**
 * SearchApi - axios parameter creator
 * @export
 */
export const SearchApiAxiosParamCreator = function (configuration?: Configuration) {
    return {
        /**
         * 
         * @param {TaskSearchViewModel} [taskSearchViewModel] 
         * @param {*} [options] Override http request option.
         * @throws {RequiredError}
         */
        search: async (taskSearchViewModel?: TaskSearchViewModel, options: AxiosRequestConfig = {}): Promise<RequestArgs> => {
            const localVarPath = `/Search`;
            // use dummy base URL string because the URL constructor only accepts absolute URLs.
            const localVarUrlObj = new URL(localVarPath, DUMMY_BASE_URL);
            let baseOptions;
            if (configuration) {
                baseOptions = configuration.baseOptions;
            }

            const localVarRequestOptions = { method: 'POST', ...baseOptions, ...options};
            const localVarHeaderParameter = {} as any;
            const localVarQueryParameter = {} as any;


    
            localVarHeaderParameter['Content-Type'] = 'application/json';

            setSearchParams(localVarUrlObj, localVarQueryParameter);
            let headersFromBaseOptions = baseOptions && baseOptions.headers ? baseOptions.headers : {};
            localVarRequestOptions.headers = {...localVarHeaderParameter, ...headersFromBaseOptions, ...options.headers};
            localVarRequestOptions.data = serializeDataIfNeeded(taskSearchViewModel, localVarRequestOptions, configuration)

            return {
                url: toPathString(localVarUrlObj),
                options: localVarRequestOptions,
            };
        },
    }
};

/**
 * SearchApi - functional programming interface
 * @export
 */
export const SearchApiFp = function(configuration?: Configuration) {
    const localVarAxiosParamCreator = SearchApiAxiosParamCreator(configuration)
    return {
        /**
         * 
         * @param {TaskSearchViewModel} [taskSearchViewModel] 
         * @param {*} [options] Override http request option.
         * @throws {RequiredError}
         */
        async search(taskSearchViewModel?: TaskSearchViewModel, options?: AxiosRequestConfig): Promise<(axios?: AxiosInstance, basePath?: string) => AxiosPromise<Array<TaskSearchResultViewModel>>> {
            const localVarAxiosArgs = await localVarAxiosParamCreator.search(taskSearchViewModel, options);
            return createRequestFunction(localVarAxiosArgs, globalAxios, BASE_PATH, configuration);
        },
    }
};

/**
 * SearchApi - factory interface
 * @export
 */
export const SearchApiFactory = function (configuration?: Configuration, basePath?: string, axios?: AxiosInstance) {
    const localVarFp = SearchApiFp(configuration)
    return {
        /**
         * 
         * @param {TaskSearchViewModel} [taskSearchViewModel] 
         * @param {*} [options] Override http request option.
         * @throws {RequiredError}
         */
        search(taskSearchViewModel?: TaskSearchViewModel, options?: any): AxiosPromise<Array<TaskSearchResultViewModel>> {
            return localVarFp.search(taskSearchViewModel, options).then((request) => request(axios, basePath));
        },
    };
};

/**
 * SearchApi - object-oriented interface
 * @export
 * @class SearchApi
 * @extends {BaseAPI}
 */
export class SearchApi extends BaseAPI {
    /**
     * 
     * @param {TaskSearchViewModel} [taskSearchViewModel] 
     * @param {*} [options] Override http request option.
     * @throws {RequiredError}
     * @memberof SearchApi
     */
    public search(taskSearchViewModel?: TaskSearchViewModel, options?: AxiosRequestConfig) {
        return SearchApiFp(this.configuration).search(taskSearchViewModel, options).then((request) => request(this.axios, this.basePath));
    }
}


