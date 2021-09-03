import React from "react";
import "./App.scss";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { useIsAuthenticated } from "@azure/msal-react";
import Header from "./layout/Header";
import { HomeScreen } from "@features/Home/HomeScreen";

import { CharactersLayout } from "@features/Characters/CharactersLayout";
import { CharactersListScreen } from "@features/Characters/CharactersListScreen";
import { CharacterEditScreen } from "@features/Characters/CharacterEditScreen";

function UnauthenticatedRoutes() : React.ReactElement {
    return (
        <Routes>
            <Route path="*" element={<HomeScreen />} />
        </Routes>
    );
}

function AuthenticatedRoutes() : React.ReactElement {
    return (
        <Routes>
            <Route path="/characters" element={<CharactersLayout />}>
                <Route path="/" element={<CharactersListScreen />} />
                <Route path=":type/:id" element={<CharacterEditScreen />} />
            </Route>
            <Route path="*" element={<HomeScreen />} />
        </Routes>
    );
}

export const App = () => {
    const isAuthenticated = useIsAuthenticated();

    return (
        <Router>
            <Header />
            <main>
                {isAuthenticated ? <AuthenticatedRoutes /> : <UnauthenticatedRoutes />}
            </main>
        </Router>
    );
}
