import React from "react";
import { useLocation, Link } from "react-router-dom";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from "@azure/msal-react";

import Navbar from "react-bootstrap/Navbar";
import Nav from "react-bootstrap/Nav";
import Button from "react-bootstrap/Button";
import ButtonGroup from "react-bootstrap/ButtonGroup";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSignInAlt, faSignOutAlt } from "@fortawesome/free-solid-svg-icons";
import { loginRequest } from "@providers/ApiClientProvider";

function Header() {
  const { pathname } = useLocation();
  const activeItem = `/${pathname.split("/")[1]}`;

  const { instance, accounts } = useMsal();

  return (
    <Navbar bg="primary" variant="dark" expand="md" className="py-1">
      <Navbar.Brand as={Link} to="/">
        Star Wars Demo
      </Navbar.Brand>
      <Navbar.Toggle />
      <Navbar.Collapse className="d-flex flex-row justify-content-between">
        <UnauthenticatedTemplate>
          <Nav className="ml-auto">
            <Nav.Item>
              {" "}
              <Button
                variant="success"
                onClick={() => instance.loginPopup(loginRequest)}
              >
                <FontAwesomeIcon icon={faSignInAlt} className="mr-1" />
                Login
              </Button>
            </Nav.Item>
          </Nav>
        </UnauthenticatedTemplate>
        <AuthenticatedTemplate>
          <Nav className="ml-auto">
            <Nav.Item>
              <Nav.Link
                as={Link}
                to="/characters"
                active={activeItem === "/characters"}
              >
                Characters
              </Nav.Link>
            </Nav.Item>
          </Nav>
          <ButtonGroup as="div">
            <span className="me-2 align-self-center text-light">
              Hello, {accounts[0]?.name}
            </span>
            <Button variant="danger" onClick={() => instance.logoutPopup()}>
              <FontAwesomeIcon icon={faSignOutAlt} className="mr-1" />
              Logout
            </Button>
          </ButtonGroup>
        </AuthenticatedTemplate>
      </Navbar.Collapse>
    </Navbar>
  );
}

export default React.memo(Header);
