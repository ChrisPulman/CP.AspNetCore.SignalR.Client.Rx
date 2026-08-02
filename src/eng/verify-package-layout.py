#!/usr/bin/env python3
"""Verify the identities, dependencies, and contents of release NuGet packages."""

from __future__ import annotations

import sys
import zipfile
from pathlib import Path
from xml.etree import ElementTree


EXPECTED_PACKAGES = {
    "CP.AspNetCore.SignalR.Client.Rx": "ReactiveUI.Primitives",
    "CP.AspNetCore.SignalR.Client.Rx.Reactive": "ReactiveUI.Primitives.Reactive",
}
EXPECTED_FRAMEWORKS = {
    "net462",
    "net472",
    "net48",
    "net481",
    "net8.0",
    "net9.0",
    "net10.0",
    "net11.0",
}
FORBIDDEN_PATH_PARTS = ("/analyzers/", "/bin/", "/obj/")
FORBIDDEN_NAMES = ("ReactiveMarbles.ObservableEvents", "SourceGenerator")


def fail(message: str) -> None:
    raise RuntimeError(message)


def local_name(tag: str) -> str:
    return tag.rsplit("}", maxsplit=1)[-1]


def child_text(parent: ElementTree.Element, name: str) -> str:
    child = next((item for item in parent if local_name(item.tag) == name), None)
    if child is None or not child.text:
        fail(f"The package manifest does not contain a {name} value.")
    return child.text.strip()


def verify_package(package_path: Path) -> str:
    with zipfile.ZipFile(package_path) as archive:
        names = archive.namelist()
        nuspec_names = [name for name in names if name.endswith(".nuspec") and "/" not in name]
        if len(nuspec_names) != 1:
            fail(f"{package_path.name} must contain exactly one root package manifest.")

        root = ElementTree.fromstring(archive.read(nuspec_names[0]))
        metadata = next((item for item in root if local_name(item.tag) == "metadata"), None)
        if metadata is None:
            fail(f"{package_path.name} does not contain package metadata.")

        package_id = child_text(metadata, "id")
        version = child_text(metadata, "version")
        if package_id not in EXPECTED_PACKAGES:
            fail(f"Unexpected package identity '{package_id}' in {package_path.name}.")
        if not package_path.name.startswith(f"{package_id}.{version}"):
            fail(f"{package_path.name} does not match manifest identity {package_id} {version}.")

        dependencies = {
            dependency.attrib["id"]
            for dependency in metadata.iter()
            if local_name(dependency.tag) == "dependency" and "id" in dependency.attrib
        }
        expected_dependency = EXPECTED_PACKAGES[package_id]
        if expected_dependency not in dependencies:
            fail(f"{package_id} must depend on {expected_dependency}.")
        if package_id.endswith(".Rx") and "System.Reactive" in dependencies:
            fail("The lean package must not depend on System.Reactive.")

        normalized_names = [f"/{name.replace(chr(92), '/')}" for name in names]
        suspect_names = [
            name
            for name in normalized_names
            if any(part in name.lower() for part in FORBIDDEN_PATH_PARTS)
            or any(forbidden.lower() in name.lower() for forbidden in FORBIDDEN_NAMES)
        ]
        if suspect_names:
            fail(f"{package_id} contains forbidden analyzer, generator, bin, or obj content: {suspect_names}")

        assembly_name = f"{package_id}.dll"
        assembly_frameworks = {
            parts[1]
            for name in names
            if len(parts := name.replace("\\", "/").split("/")) == 3
            and parts[0] == "lib"
            and parts[2] == assembly_name
        }
        if assembly_frameworks != EXPECTED_FRAMEWORKS:
            fail(
                f"{package_id} framework assemblies were {sorted(assembly_frameworks)}; "
                f"expected {sorted(EXPECTED_FRAMEWORKS)}."
            )

        other_project_assemblies = [
            name
            for name in names
            if name.endswith(".dll")
            and "/lib/" in f"/{name.replace(chr(92), '/')}"
            and not name.endswith(assembly_name)
        ]
        if other_project_assemblies:
            fail(f"{package_id} contains unexpected library assemblies: {other_project_assemblies}")

        return package_id


def main() -> int:
    package_directory = Path(sys.argv[1] if len(sys.argv) > 1 else "packages").resolve()
    if not package_directory.is_dir():
        fail(f"Package directory does not exist: {package_directory}")

    package_paths = sorted(
        path
        for path in package_directory.glob("*.nupkg")
        if not path.name.endswith(".symbols.nupkg")
    )
    verified_ids = {verify_package(package_path) for package_path in package_paths}
    expected_ids = set(EXPECTED_PACKAGES)
    if verified_ids != expected_ids:
        fail(f"Verified package identities were {sorted(verified_ids)}; expected {sorted(expected_ids)}.")

    print(f"Verified package layout for: {', '.join(sorted(verified_ids))}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except RuntimeError as error:
        print(f"Package verification failed: {error}", file=sys.stderr)
        raise SystemExit(1) from error
