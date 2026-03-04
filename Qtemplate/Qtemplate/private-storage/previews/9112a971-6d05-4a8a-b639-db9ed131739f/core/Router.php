<?php
// core/Router.php

class Router {
    private $routes = [];
    
    public function add($method, $path, $controller, $action) {
        $this->routes[] = [
            'method' => strtoupper($method),
            'path' => $path,
            'controller' => $controller,
            'action' => $action
        ];
    }
    
    public function dispatch($requestMethod, $requestUri) {
        // Remove query string
        $uri = strtok($requestUri, '?');
        
        // Get the script name to remove it from URI
        $scriptName = dirname($_SERVER['SCRIPT_NAME']);
        
        // Remove script name from URI if exists
        if ($scriptName !== '/' && strpos($uri, $scriptName) === 0) {
            $uri = substr($uri, strlen($scriptName));
        }
        
        // Remove common API prefixes
        $prefixes = [
            '/api/' . Config::API_VERSION,
            '/api/v1',
            '/api',
            '/index.php'
        ];
        
        foreach ($prefixes as $prefix) {
            if (strpos($uri, $prefix) === 0) {
                $uri = substr($uri, strlen($prefix));
                break;
            }
        }
        
        // Ensure uri starts with /
        if (empty($uri)) {
            $uri = '/';
        } elseif ($uri[0] !== '/') {
            $uri = '/' . $uri;
        }
        
        // Debug log
        error_log("===========================================");
        error_log("Original URI: " . $requestUri);
        error_log("Processed URI: " . $uri);
        error_log("Request Method: " . $requestMethod);
        error_log("Script Name: " . $scriptName);
        
        foreach ($this->routes as $route) {
            // Match method
            if ($route['method'] !== $requestMethod) {
                continue;
            }
            
            // Convert route path to regex pattern
            $pattern = preg_replace('/\{[a-zA-Z0-9_]+\}/', '([a-zA-Z0-9_-]+)', $route['path']);
            $pattern = '#^' . $pattern . '$#';
            
            error_log("Testing route: " . $route['method'] . " " . $route['path'] . " (pattern: " . $pattern . ")");
            
            // Match path
            if (preg_match($pattern, $uri, $matches)) {
                array_shift($matches); // Remove full match
                
                error_log("✓ Route MATCHED! Controller: " . $route['controller'] . ", Action: " . $route['action']);
                
                // Instantiate controller and call action
                $controllerName = $route['controller'];
                $actionName = $route['action'];
                
                if (!class_exists($controllerName)) {
                    error_log("ERROR: Controller class not found: " . $controllerName);
                    Response::error('Controller not found: ' . $controllerName, 500);
                }
                
                $controller = new $controllerName();
                
                if (!method_exists($controller, $actionName)) {
                    error_log("ERROR: Method not found: " . $actionName);
                    Response::error('Method not found: ' . $actionName, 500);
                }
                
                call_user_func_array([$controller, $actionName], $matches);
                return;
            }
        }
        
        // No route matched
        error_log("✗ No route matched for: " . $uri);
        error_log("Available routes:");
        foreach ($this->routes as $route) {
            error_log("  - " . $route['method'] . " " . $route['path']);
        }
        
        Response::notFound('Route not found: ' . $uri . ' (Method: ' . $requestMethod . ')');
    }
    
    public function get($path, $controller, $action) {
        $this->add('GET', $path, $controller, $action);
    }
    
    public function post($path, $controller, $action) {
        $this->add('POST', $path, $controller, $action);
    }
    
    public function put($path, $controller, $action) {
        $this->add('PUT', $path, $controller, $action);
    }
    
    public function delete($path, $controller, $action) {
        $this->add('DELETE', $path, $controller, $action);
    }
}